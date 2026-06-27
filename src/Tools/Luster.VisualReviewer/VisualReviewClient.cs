using System;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Luster.VisualReviewer
{
    /// <summary>
    /// 调 siliconflow Qwen3-VL-8B 评阅 WPF 截图,产结构化 JSON 报告;
    /// 网络/key 失败自动降级(Degraded=true,不抛),截图已由调用方落盘。
    /// </summary>
    public sealed class VisualReviewClient : IVisualReviewClient
    {
        private const string Endpoint = "https://api.siliconflow.cn/v1/chat/completions";
        private const string Model = "Qwen/Qwen3-VL-8B-Instruct";

        private readonly string _apiKey;

        /// <summary>构造,apiKey 由调用方注入(Task 7 从环境变量取)</summary>
        public VisualReviewClient(string apiKey)
        {
            _apiKey = apiKey ?? "";
        }

        /// <summary>评审截图;失败降级返回 Degraded 报告,不抛</summary>
        public ReviewReport Review(byte[] png, string viewName)
        {
            try
            {
                string content = CallModel(png);
                return ParseReport(content, viewName);
            }
            catch (Exception ex)
            {
                // 降级:不抛,标 Degraded;截图已由调用方落盘
                return new ReviewReport
                {
                    View = viewName,
                    Degraded = true,
                    Summary = "视觉模型不可达: " + ex.Message,
                    Score = -1
                };
            }
        }

        /// <summary>
        /// 把模型返回的 content 字符串解析为 ReviewReport。
        /// 兼容纯 JSON 与 markdown 代码块包裹(```json ... ```)两种返回形态。
        /// 供测试直接调用,不走网络。
        /// </summary>
        public static ReviewReport ParseReport(string json, string viewName)
        {
            var report = new ReviewReport { View = viewName };
            string normalized = StripCodeFence(json);
            JObject root = JObject.Parse(normalized);
            report.Summary = (string)root["summary"] ?? "";
            report.Score = (int)(root["score"] ?? 0);
            foreach (var item in root["issues"] ?? new JArray())
            {
                // 优化 prompt 模型返回 pass/warn/fail;映射到 ReviewReport 期望的 high/medium/low。
                // pass 不进 issues(优化 prompt 第4条已要求,此处防御性再过滤一次)。
                // 兼容已存在的 high/medium/low 直接透传(保原大小写)。
                string raw = ((string)item["severity"] ?? "low").Trim();
                string key = raw.ToLowerInvariant();
                if (key == "pass") continue;
                string severity;
                if (key == "fail") severity = "high";
                else if (key == "warn") severity = "medium";
                else severity = raw;  // high/medium/low 或其他直接透传
                report.Issues.Add(new ReviewIssue
                {
                    Severity = severity,
                    Category = (string)item["category"] ?? "",
                    Description = (string)item["description"] ?? "",
                    Location = (string)item["location"] ?? ""
                });
            }
            return report;
        }

        /// <summary>剥除 ```json / ``` 代码块包裹,返回可被 JObject.Parse 直解的 JSON 文本</summary>
        private static string StripCodeFence(string raw)
        {
            if (string.IsNullOrEmpty(raw)) return raw ?? "";
            string s = raw.Trim();
            if (s.StartsWith("```"))
            {
                // 去掉首行 ```json 或 ```
                int firstNewline = s.IndexOf('\n');
                if (firstNewline >= 0) s = s.Substring(firstNewline + 1);
                else s = s.Substring(3);
                // 去掉末尾 ```
                int lastFence = s.LastIndexOf("```", StringComparison.Ordinal);
                if (lastFence >= 0) s = s.Substring(0, lastFence);
                s = s.Trim();
            }
            return s;
        }

        /// <summary>调 siliconflow OpenAI 兼容接口,返回 choices[0].message.content</summary>
        /// <remarks>不再接 contract:优化 prompt 自包含像素可见维度,不依赖契约全文。
        /// 契约含源码级维度(hc:前缀/资源键/校验样式),视觉模型从像素看不到,
        /// 套契约条款只会瞎猜源码级假问题。源码级合规另走 Luster.XamlLinter 静态解析。</remarks>
        private string CallModel(byte[] png)
        {
            if (string.IsNullOrEmpty(_apiKey))
                throw new InvalidOperationException("API key 未配置(VisualReviewClient)");

            string base64 = Convert.ToBase64String(png ?? new byte[0]);
            // 优化 prompt(实验验证,见 workspace/model-compare/compare2.py):只评像素可见维度,
            // 禁源码级判断,要求 evidence。不再把整份设计契约拼进 prompt(那是套模板的根源)。
            string prompt = @"你是工业 WPF 界面视觉评审。重要约束:
1. 你只能看到截图渲染后的像素,看不到 XAML 源码。因此不要评论“是否用了某控件库(hc:/HandyControl)”“是否引用了资源键{StaticResource}”“是否有校验样式”这类源码级细节——你从像素无法判断这些,猜测只会出错。
2. 仅基于截图实际看到的内容评审,不要套用通用模板。看不出问题就 pass,不要为凑数编造问题。
3. 只评以下像素可见维度,每项 verdict(pass/warn/fail)+ evidence(截图里实际看到的客观描述):
   - overlap:控件有无重叠、文字被遮挡、内容被截断
   - spacing:留白/间距是否一致、有无突兀贴边或过大空隙
   - layout:控件对齐、分区是否清晰、信息密度是否合理
   - font:字号视觉大小是否协调(标题>正文>标签)、有无过小看不清的文字
4. score 0-10,10=视觉无瑕疵。issues 只列 warn/fail 项(pass 项不进 issues)。
5. summary 必填,用一句中文总结整体观感(不得为空串)。若 score≥9 且 issues 为空,summary 必须说明判定无瑕的具体依据(如分区清晰、字号协调、无重叠);若 score≤6,summary 须点出主要扣分项。
6. 只返回 JSON,不要 markdown 包裹:
{""summary"":""一句整体观感"",""score"":0,""issues"":[{""severity"":"""",""category"":"""",""description"":"""",""location"":""""}]}";
            var body = new
            {
                model = Model,
                max_tokens = 1024,
                messages = new[]
                {
                    new
                    {
                        role = "user",
                        content = new object[]
                        {
                            new { type = "text", text = prompt },
                            new { type = "image_url", image_url = new { url = "data:image/png;base64," + base64 } }
                        }
                    }
                }
            };
            using (var client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(60);
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + _apiKey);
                var content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");
                var resp = client.PostAsync(Endpoint, content).Result;
                resp.EnsureSuccessStatusCode();
                var respStr = resp.Content.ReadAsStringAsync().Result;
                // OpenAI 兼容:取 choices[0].message.content
                var j = JObject.Parse(respStr);
                return (string)j["choices"][0]["message"]["content"];
            }
        }
    }
}
