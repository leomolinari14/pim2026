(() => {
  'use strict';
  // A integração oficial carrega e inicializa o VLibras automaticamente.
  // No celular, o botão fica no rodapé, afastado do formulário e do cabeçalho.
  const posicionar = () => {
    const raiz = document.getElementById('vlibras-access-wrapper')?.shadowRoot;
    if (!raiz) return false;
    if (!raiz.getElementById('pim-posicao-vlibras')) {
      const estilo = document.createElement('style');
      estilo.id = 'pim-posicao-vlibras';
      estilo.textContent = '@media (max-width: 767.98px) { #vlibras-access { top: auto !important; bottom: 16px !important; transform: none !important; } }';
      raiz.appendChild(estilo);
    }
    return true;
  };
  if (posicionar()) return;
  const observador = new MutationObserver(() => { if (posicionar()) observador.disconnect(); });
  observador.observe(document.body, {childList:true,subtree:true});
  window.addEventListener('load', () => { if (posicionar()) observador.disconnect(); }, {once:true});
  setTimeout(() => observador.disconnect(), 15000);
})();
