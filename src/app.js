(function (global) {
  const createCounter = typeof require === 'function'
    ? require('./counter').createCounter
    : global.createCounter;

  function startCounterApp(document) {
    const counter = createCounter();
    const countValue = document.getElementById('count-value');
    const incrementButton = document.getElementById('increment-button');

    countValue.textContent = String(counter.value);
    incrementButton.addEventListener('click', () => {
      countValue.textContent = String(counter.increment());
    });
  }

  if (global.document) {
    startCounterApp(global.document);
  }

  if (typeof module !== 'undefined' && module.exports) {
    module.exports = { startCounterApp };
  }
})(typeof window !== 'undefined' ? window : globalThis);
