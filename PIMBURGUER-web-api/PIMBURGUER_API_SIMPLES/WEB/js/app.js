/* Navegação por hash: funciona em index.html, Live Server e no iniciador do pacote. */
(() => {
  'use strict'; const P=window.Pim;
  const dynamic=document.getElementById('pagina-dinamica'), catalog=document.getElementById('pagina-cardapio');
  const aviso=document.getElementById('aviso-carrinho');
  let avisoTimer;
  const fecharAviso=()=>{clearTimeout(avisoTimer);aviso.hidden=true;};
  const agendarAviso=()=>{
    clearTimeout(avisoTimer);
    if(!aviso.hidden&&!aviso.matches(':hover')&&!aviso.contains(document.activeElement)) avisoTimer=setTimeout(fecharAviso,8000);
  };
  const mostrarAviso=nome=>{
    clearTimeout(avisoTimer);
    aviso.querySelector('[data-aviso-texto]').textContent=nome+' adicionado ao carrinho.';
    aviso.hidden=false;agendarAviso();
  };
  aviso.querySelector('[data-fechar-aviso]').onclick=fecharAviso;
  aviso.addEventListener('mouseenter',()=>clearTimeout(avisoTimer));
  aviso.addEventListener('mouseleave',agendarAviso);
  aviso.addEventListener('focusin',()=>clearTimeout(avisoTimer));
  aviso.addEventListener('focusout',()=>setTimeout(agendarAviso,0));
  window.addEventListener('hashchange',fecharAviso);
  P.render=html=>{dynamic.innerHTML=html; dynamic.hidden=false; catalog.hidden=true;};
  P.updateHeader=()=>{
    const link=document.getElementById('conta-link');
    link.href=P.user()?(P.admin()?'#/admin':'#/cliente'):'#/conta/login';
    link.querySelector('span').textContent=P.user()?'Minha conta':'Entrar';
    link.setAttribute('aria-label',P.user()?'Abrir minha conta':'Entrar na conta');
    document.querySelectorAll('[data-cart-count]').forEach(x=>{x.textContent=P.cart.reduce((sum,item)=>sum+item.quantidade,0);});
  };
  P.route=async ()=>{
    const original=location.hash;
    const [path,search='']=(location.hash.replace(/^#\/?/,'')||'').split('?');
    const parts=path.toLowerCase().split('/'), query=new URLSearchParams(search);
    const section=parts[0],page=parts[1],id=parts[2];
    if(section==='conteudo') { document.getElementById('conteudo').focus(); return; }
    P.navigationId=(P.navigationId||0)+1;
    const titles={'conta':'Minha conta','admin':'Administração','cliente':'Meu painel','checkout':'Finalizar pedido','carrinho':'Meu pedido','confirmacao':'Pedido recebido'};
    document.title=(titles[section]||'Cardápio')+' | PimBurguer';
    P.updateHeader();
    if(!section||section==='cardapio') {
      dynamic.hidden=true; catalog.hidden=false;
      await P.catalog();
      if(section==='cardapio') document.getElementById('cardapio').scrollIntoView();
      return;
    }
    P.render('<div class="page-loading" role="status">Carregando…</div>');
    try {
      if(section==='conta' && page==='login') P.loginPage(false);
      else if(section==='conta' && page==='cadastro') P.loginPage(true);
      else if(section==='conta' && page==='perfil') await P.profilePage();
      else if(section==='carrinho') await P.cartPage();
      else if(section==='checkout') await P.checkoutPage();
      else if(section==='confirmacao'&&page) await P.confirmation(page);
      else if(section==='admin'||section==='cliente') {
        const admin=section==='admin';
        if(!P.requireLogin(admin)) return;
        if(!page) await P.dashboard(admin);
        else if(page==='pedidos'&&id) await P.orderPage(admin,id);
        else if(page==='pedidos') await P.ordersPage(admin,query);
        else if(page==='cupons'&&id&&admin) await P.couponPage(id);
        else if(page==='cupons') await P.couponsPage(admin);
        else if(page==='produtos'&&id&&admin) await P.productPage(id);
        else if(page==='produtos'&&admin) await P.productsPage();
        else if(page==='clientes'&&admin) await P.clientsPage();
        else if(page==='relatorios'&&admin) await P.reportsPage(query);
        else throw new Error('Página não encontrada.');
      } else throw new Error('Página não encontrada.');
      if(location.hash===original) { window.scrollTo(0,0); document.getElementById('conteudo').focus({preventScroll:true}); }
    } catch(error) {
      if(error.navigation || location.hash!==original) return;
      P.render(`<section class="shopping-page"><div class="container"><div class="retry-box"><h1 class="h3">Não foi possível abrir esta página.</h1><p>${P.escape(error.message)}</p><div class="inline-actions"><button class="btn btn-primary" id="tentar-pagina">Tentar novamente</button><a class="btn btn-outline-primary" href="#/">Voltar ao cardápio</a>${!P.user()?'<a href="#/conta/login">Entrar na conta</a>':''}</div></div></div></section>`);
      document.getElementById('tentar-pagina').onclick=P.route;
    }
  };
  document.getElementById('ano').textContent=new Date().getFullYear();
  document.getElementById('busca-form').onsubmit=event=>{event.preventDefault();P.filterProducts();};
  document.getElementById('buscar-produto').oninput=P.filterProducts;
  document.addEventListener('click',async event=>{
    const button=event.target.closest('button'); if(!button) return;
    try {
      if(button.hasAttribute('data-categoria')) { P.category=button.dataset.categoria; P.filterProducts(); }
      if(button.hasAttribute('data-add')) {
        const id=Number(button.dataset.add), item=P.cart.find(x=>x.produtoId===id);
        P.changeCart(id,(item?.quantidade||0)+1);
        mostrarAviso(P.products.find(product=>product.id===id)?.nome||'Produto');
      }
      if(button.hasAttribute('data-quantity')) {
        P.changeCart(Number(button.dataset.quantity),Number(button.dataset.value)); button.disabled=true; await P.cartPage();
      }
      if(button.hasAttribute('data-action')) { button.disabled=true; await P.action(button); }
    } catch(error) { P.message(error.message,'danger'); }
    finally { button.disabled=false; }
  });
  window.addEventListener('hashchange',P.route);
  // Configuração pública contém somente nome, taxa e WhatsApp; nenhuma senha.
  P.request('/api/loja').then(loja=>{
    if(/^55\d{10,11}$/.test(loja.whatsApp||'')) document.querySelectorAll('[data-whatsapp]').forEach(link=>{link.href='https://wa.me/'+loja.whatsApp;link.hidden=false;});
  }).catch(()=>{});
  P.route();
})();
