const test = require('node:test');
const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const vm = require('node:vm');

test('浏览器页面点击按钮后数字递增', () => {
  const countValue = { textContent: '' };
  let clickHandler = null;
  const incrementButton = {
    addEventListener(eventName, handler) {
      if (eventName === 'click') {
        clickHandler = handler;
      }
    },
  };
  const document = {
    getElementById(id) {
      if (id === 'count-value') return countValue;
      if (id === 'increment-button') return incrementButton;
      return null;
    },
  };
  const sandbox = { window: { document }, document };
  vm.createContext(sandbox);

  const counterScript = fs.readFileSync(path.join(__dirname, '../src/counter.js'), 'utf8');
  const appScript = fs.readFileSync(path.join(__dirname, '../src/app.js'), 'utf8');

  vm.runInContext(counterScript, sandbox);
  vm.runInContext(appScript, sandbox);

  assert.equal(countValue.textContent, '0');
  clickHandler();
  clickHandler();
  assert.equal(countValue.textContent, '2');
});
