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
        public ReviewReport Review(byte[] png, string contract, string viewName)
        {
            try
            {
                string content = CallModel(png, contract);
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
                report.Issues.Add(new ReviewIssue
                {
                    Severity = (string)item["severity"] ?? "low",
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
        private string CallModel(byte[] png, string contract)
        {
            if (string.IsNullOrEmpty(_apiKey))
                throw new InvalidOperationException("API key 未配置(VisualReviewClient)");

            string base64 = Convert.ToBase64String(png ?? new byte[0]);
            string prompt = "你是工业 WPF 界面评审。按以下设计契约评阅截图,只返回 JSON,不要 markdown 包裹:" +
                            "{\"summary\":\"\",\"score\":0,\"issues\":[{\"severity\":\"\",\"category\":\"\",\"description\":\"\",\"location\":\"\"}]}\n" +
                            "契约:\n" + (contract ?? "");
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
