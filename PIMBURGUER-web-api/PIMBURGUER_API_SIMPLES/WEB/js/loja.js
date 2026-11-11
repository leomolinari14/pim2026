/* Cardápio, carrinho e checkout. Preços e descontos são sempre calculados na API. */
(() => {
  'use strict'; const P=window.Pim, E=P.escape;
  const saved=P.read(localStorage,'pim-carrinho',[]);
  P.cart=Array.isArray(saved) ? saved.filter(x=>Number.isInteger(x.produtoId)&&x.produtoId>0&&Number.isInteger(x.quantidade)&&x.quantidade>0&&x.quantidade<=20).slice(0,30) : [];
  P.products=[]; P.category='';
  P.persistCart=() => { localStorage.setItem('pim-carrinho',JSON.stringify(P.cart)); P.updateHeader?.(); };
  P.changeCart=(id,quantity) => {
    const items=P.cart.filter(x=>x.produtoId!==id);
    if(quantity>0) items.push({produtoId:id,quantidade:quantity});
    if(quantity>20 || items.length>30 || items.reduce((sum,x)=>sum+x.quantidade,0)>100) throw new Error('Limite de 20 unidades por produto, 30 produtos e 100 unidades por pedido.');
    P.cart=items; P.persistCart();
  };
  P.catalog=async () => {
    const status=document.getElementById('catalogo-status');
    status.textContent='Carregando cardápio…';
    try { P.products=await P.request('/api/produtos'); P.filterProducts(); }
    catch(error) {
      if(error.navigation) return;
      document.getElementById('lista-produtos').innerHTML='';
      status.innerHTML=`<div class="retry-box"><h3>Vamos conectar sua loja.</h3><p>${E(error.message)}</p><p>Confira se a API está iniciada e conectada ao PostgreSQL.</p><button class="btn btn-primary" id="reconectar">Tentar novamente</button></div>`;
      document.getElementById('reconectar').onclick=P.catalog;
    }
  };
  P.filterProducts=() => {
    const query=document.getElementById('buscar-produto').value.trim();
    const normalize=value=>value.normalize('NFD').replace(/[\u0300-\u036f]/g,'').toLowerCase();
    const categories=[...new Set(P.products.map(x=>x.categoria))];
    if(!categories.includes(P.category)) P.category='';
    document.getElementById('categorias').innerHTML=['',...categories].map(c=>`<button type="button" data-categoria="${E(c)}" aria-pressed="${P.category===c}">${E(c||'Todos')}</button>`).join('');
    const filtered=P.products.filter(x=>(!P.category||x.categoria===P.category)&&normalize(x.nome+' '+x.descricao).includes(normalize(query)));
    document.getElementById('catalogo-status').textContent=`${filtered.length} ${filtered.length===1?'produto':'produtos'}${query?' para “'+query+'”':''} no cardápio.`;
    document.getElementById('lista-produtos').innerHTML=filtered.length ? filtered.map(product=>`<div class="col"><article class="card product-card h-100" aria-labelledby="produto-${product.id}"><img class="card-img-top" src="${E(P.image(product.imagem))}" alt="${E(product.nome)}" loading="lazy" width="660" height="400"><div class="card-body d-flex flex-column"><p class="product-category">${E(product.categoria)}</p><h3 class="card-title" id="produto-${product.id}">${E(product.nome)}</h3><p class="card-text">${E(product.descricao)}</p><div class="product-buy mt-auto"><p class="product-price mb-0">${P.money(product.preco)}</p><button type="button" class="btn btn-primary" data-add="${product.id}" aria-label="Adicionar ${E(product.nome)} ao pedido">Adicionar</button></div></div></article></div>`).join('') : P.empty('Nenhum produto encontrado.','<p>Experimente outro nome ou ingrediente.</p>');
  };
  P.summary=quote=>`<dl class="summary-lines"><div><dt>Subtotal</dt><dd>${P.money(quote.subtotal)}</dd></div><div><dt>Entrega</dt><dd>${quote.taxaEntrega===0?'Grátis':P.money(quote.taxaEntrega)}</dd></div>${quote.desconto?`<div><dt>Desconto ${E(quote.codigoCupom)}</dt><dd>− ${P.money(quote.desconto)}</dd></div>`:''}<div class="total"><dt>Total</dt><dd>${P.money(quote.total)}</dd></div></dl>`;
  P.cartPage=async () => {
    if(!P.cart.length) {
      P.render(`<section class="shopping-page"><div class="container">${P.empty('Seu pedido começa aqui.','<p>Escolha seus favoritos para começar.</p><a class="btn btn-primary" href="#/cardapio">Explorar o cardápio</a>')}</div></section>`); return;
    }
    // A lista atual identifica itens que ficaram indisponíveis, sem esvaziar o carrinho.
    P.products=await P.request('/api/produtos');
    const available=new Map(P.products.map(x=>[x.id,x]));
    let quote=null, error='';
    try { quote=await P.request('/api/pedidos/orcamento',{method:'POST',body:{itens:P.cart,recebimento:'Retirada'}}); }
    catch(e) { error=e.message; }
    P.render(`<section class="shopping-page"><div class="container"><nav class="purchase-steps" aria-label="Etapas do pedido"><strong>1. Carrinho</strong><span>2. Dados e pagamento</span><span>3. Confirmação</span></nav><div class="shopping-heading"><div><p class="eyebrow">Do seu jeito</p><h1>Meu pedido</h1></div><a href="#/cardapio">Continuar escolhendo</a></div>${error?`<p class="alert alert-warning">${E(error)}</p>`:''}<div class="row g-4"><div class="col-lg-8"><div class="shopping-list">${P.cart.map(item=>{
      const product=available.get(item.produtoId);
      return `<article class="cart-item">${product?`<img src="${E(P.image(product.imagem))}" alt="${E(product.nome)}" width="100" height="100">`:''}<div class="cart-item-info"><h2>${E(product?.nome||'Produto indisponível')}</h2><p>${product?P.money(product.preco)+' cada':'Remova este item para continuar.'}</p><div class="cart-item-actions"><div class="quantity-control"><button type="button" data-quantity="${item.produtoId}" data-value="${item.quantidade-1}" aria-label="Diminuir quantidade">−</button><span>${item.quantidade}</span><button type="button" data-quantity="${item.produtoId}" data-value="${item.quantidade+1}" aria-label="Aumentar quantidade" ${item.quantidade>=20?'disabled':''}>+</button></div><button type="button" class="action-link" data-quantity="${item.produtoId}" data-value="0">Remover</button></div></div><strong>${product?P.money(product.preco*item.quantidade):'—'}</strong></article>`;
    }).join('')}</div></div><div class="col-lg-4"><aside class="order-summary"><h2>Resumo do pedido</h2>${quote?P.summary(quote):'<p>Revise os itens indisponíveis.</p>'}<p class="small">Escolha entrega ou retirada na próxima etapa.</p>${quote?'<a class="btn btn-primary w-100" href="#/checkout">Continuar para o checkout</a>':''}</aside></div></div></div></section>`);
  };
  P.checkoutPage=async () => {
    if(!P.requireLogin()) return;
    if(!P.cart.length) { location.hash='#/carrinho'; return; }
    const user=await P.request('/api/conta/me');
    P.render(P.template('checkout'));
    const form=document.getElementById('checkout-form');
    for(const [key,value] of Object.entries({nomeCliente:user.nome,telefone:user.telefone,cpfCliente:user.cpf||''})) form.elements[key].value=value;
    P.initCpf(form);
    let quote=null, coupon='', sequence=0;
    const button=document.getElementById('confirmar-pedido');
    const refresh=async () => {
      const current=++sequence; quote=null; button.disabled=true;
      document.getElementById('checkout-resumo').textContent='Atualizando valores…';
      try {
        const result=await P.request('/api/pedidos/orcamento',{method:'POST',body:{itens:P.cart,recebimento:form.elements.recebimento.value,codigoCupom:coupon||null}});
        if(current!==sequence) return;
        quote=result; document.getElementById('checkout-resumo').innerHTML=P.summary(quote); button.disabled=false;
      } catch(error) {
        if(current===sequence) document.getElementById('checkout-resumo').innerHTML=`<p class="text-danger">${E(error.message)}</p>`;
        throw error;
      }
    };
    form.elements.recebimento.onchange=async () => {
      const delivery=form.elements.recebimento.value==='Entrega';
      const address=document.getElementById('endereco'); address.hidden=!delivery; address.disabled=!delivery;
      try { await refresh(); } catch(error) { P.message(error.message,'danger'); }
    };
    form.elements.pagamento.onchange=() => {
      const cash=form.elements.pagamento.value==='Dinheiro';
      document.getElementById('troco-campo').hidden=!cash; form.elements.trocoPara.disabled=!cash;
    };
    P.form(document.getElementById('cupom-form'),async data => { coupon=data.codigoCupom.trim(); await refresh(); P.message(coupon?'Cupom aplicado ao orçamento.':'Cupom removido.'); });
    P.form(form,async data => {
      if(!quote) throw new Error('Aguarde o cálculo dos valores antes de confirmar.');
      const input={...data,itens:P.cart.map(x=>({...x})),codigoCupom:coupon||null,trocoPara:data.trocoPara?Number(data.trocoPara):null};
      // Mantém ID e token nas tentativas do mesmo pedido, inclusive após perda de conexão.
      const fingerprint=JSON.stringify(input);
      let pending=P.read(sessionStorage,'pim-pedido-pendente');
      if(!pending || pending.fingerprint!==fingerprint) {
        const bytes=crypto.getRandomValues(new Uint8Array(32));
        pending={fingerprint,id:crypto.randomUUID(),token:Array.from(bytes,b=>b.toString(16).padStart(2,'0')).join('')};
        sessionStorage.setItem('pim-pedido-pendente',JSON.stringify(pending));
      }
      try {
        const order=await P.request('/api/pedidos',{method:'POST',body:{...input,id:pending.id,tokenConsulta:pending.token,orcamentoHash:quote.hash}});
        sessionStorage.setItem('pim-ultimo-pedido',JSON.stringify({id:order.id,token:pending.token}));
        sessionStorage.removeItem('pim-pedido-pendente'); P.cart=[]; P.persistCart();
        location.hash='#/confirmacao/'+order.id;
      } catch(error) {
        if(error.status===409) { await refresh(); throw new Error('O pedido mudou. Confira os novos valores e confirme novamente.'); }
        throw error;
      }
    });
    await refresh();
  };
  P.orderDetail=order=>`<div class="panel-card"><div class="inline-actions mb-3">${P.pill(order.status)}${P.pill(order.pago?'Pago':'Pagamento pendente',order.pago?'is-success':'')}</div><p><strong>Pedido #${E(order.id.slice(0,8))}</strong> · ${P.date(order.criadoEm)}</p><p>${E(order.nomeCliente)} · ${E(order.telefone)}${order.cpfCliente?'<br>CPF '+E(order.cpfCliente):''}</p><p>${E(order.recebimento)}${order.endereco?': '+E(order.endereco):''}<br>Pagamento: ${E(({CartaoDebito:'Cartão de débito',CartaoCredito:'Cartão de crédito'})[order.pagamento]||order.pagamento)}${order.trocoPara?'<br>Troco para '+P.money(order.trocoPara):''}</p>${order.observacoes?'<p>Observações: '+E(order.observacoes)+'</p>':''}${order.itens.map(item=>`<div class="order-item-line"><span>${item.quantidade} × ${E(item.nome)}<small>${P.money(item.precoUnitario)} cada</small></span><strong>${P.money(item.subtotal)}</strong></div>`).join('')}${P.summary(order)}</div>`;
  P.confirmation=async id => {
    if(!P.requireLogin()) return;
    const order=await P.request('/api/cliente/pedidos/'+encodeURIComponent(id));
    const whatsapp=/^https:\/\/wa\.me\/55\d{10,11}\?/.test(order.whatsAppUrl||'') ? order.whatsAppUrl : null;
    P.render(`<section class="shopping-page"><div class="container" style="max-width:850px"><div class="confirmation-icon" aria-hidden="true">✓</div><p class="eyebrow">Tudo certo</p><h1>Pedido recebido!</h1><p>Seu pedido está registrado na loja. Acompanhe o andamento pelo seu painel.</p>${P.orderDetail(order)}<div class="inline-actions mt-4">${whatsapp?`<a class="btn btn-primary" href="${E(whatsapp)}" target="_blank" rel="noopener noreferrer">Enviar resumo pelo WhatsApp</a>`:''}<a class="btn btn-outline-primary" href="#/cliente/pedidos">Acompanhar meus pedidos</a><a href="#/cardapio">Voltar ao cardápio</a></div></div></section>`);
  };
})();
