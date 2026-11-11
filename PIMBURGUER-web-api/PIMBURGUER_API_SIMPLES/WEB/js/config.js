// A API usa a porta 5181 no desenvolvimento.
// Se o site for hospedado em outro endereço, ajuste apiUrl.
window.PIM_CONFIG = {
  apiUrl: location.protocol !== 'file:' && location.port === '5181'
    ? '' : 'http://localhost:5181'
};
