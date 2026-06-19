const test = require('node:test');
const assert = require('node:assert/strict');

const { createCounter } = require('../src/counter');

test('计数器初始值为 0', () => {
  const counter = createCounter();

  assert.equal(counter.value, 0);
});

test('每点击一次计数持续递增', () => {
  const counter = createCounter();

  assert.equal(counter.increment(), 1);
  assert.equal(counter.increment(), 2);
  assert.equal(counter.increment(), 3);
  assert.equal(counter.value, 3);
});
