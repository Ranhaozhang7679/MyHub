(function (global) {
  function createCounter() {
    let value = 0;

    return {
      get value() {
        return value;
      },
      increment() {
        value += 1;
        return value;
      },
    };
  }

  if (typeof module !== 'undefined' && module.exports) {
    module.exports = { createCounter };
  } else {
    global.createCounter = createCounter;
  }
})(typeof window !== 'undefined' ? window : globalThis);
