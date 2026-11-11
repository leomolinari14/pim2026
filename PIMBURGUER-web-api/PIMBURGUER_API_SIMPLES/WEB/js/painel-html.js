/* Painéis HTML de cliente e administração, usando os contratos reais da API. */
(() => {
  'use strict'; const P=window.Pim, E=P.escape;
  const statuses=['Aguardando confirmação','Confirmado','Em preparo','Saiu para entrega','Pronto para retirada','Concluído','Cancelado'];
  P.panel=(html, active='') => {
    const admin=P.admin(), prefix=admin?'admin':'cliente';
    const links=admin ? [['','Visão geral','grid'],['produtos','Cardápio','bag'],['pedidos','Pedidos','orders'],['cupons','Cupons','coupon'],['relatorios','Relatórios','chart'],['clientes','Clientes','user']]
      : [['','Meu painel','grid'],['pedidos','Meus pedidos','orders'],['cupons','Meus cupons','coupon']];
    P.render(`<div class="panel-shell"><aside class="panel-sidebar"><p class="panel-caption">${admin?'Gestão da loja':'Sua conta'}</p><p class="panel-greeting">Olá, ${E(P.user().nome)}</p><nav class="panel-navigation" aria-label="Navegação do painel">${links.map(([path,label,icon])=>`<a href="#/${prefix}${path?'/'+path:''}" ${active===path?'class="is-active" aria-current="page"':''}>${P.icon(icon)}${label}</a>`).join('')}<a href="#/conta/perfil" ${active==='perfil'?'class="is-active"':''}>${P.icon('user')}Meus dados</a><a href="#/cardapio">${P.icon('arrow')}Ir ao cardápio</a></nav><div class="panel-logout"><button type="button" data-action="sair">${P.icon('exit')}Sair da conta</button></div></aside><div class="panel-content">${html}</div></div>`);
  };
  P.dashboard=async admin => {
    if(!P.requireLogin(admin)) return;
    const data=await P.request(admin?'/api/admin/relatorios':'/api/cliente/resumo');
    const metrics=admin ? [['Pedidos no período',data.totalPedidos],['Valor dos pedidos',P.money(data.valorPedidos)],['Recebido',P.money(data.valorRecebido)],['Ticket médio',P.money(data.ticketMedio)]]
      : [['Meus pedidos',data.totalPedidos],['Em andamento',data.emAndamento],['Concluídos',data.concluidos],['Valor dos pedidos',P.money(data.valorPedidos)]];
    P.panel(P.heading(admin?'Visão geral':'Meu painel',admin?'Resultados dos últimos 30 dias.':'Acompanhe seus pedidos e aproveite os cupons da casa.')+P.metrics(metrics)+`<div class="client-offer"><div><h2>${admin?'Tudo para cuidar da sua loja.':'Qual vai ser o pedido de hoje?'}</h2><p>${admin?'Gerencie o cardápio, os pedidos e os clientes.':'Escolha seus favoritos e acompanhe por aqui.'}</p></div><a class="btn btn-primary" href="${admin?'#/admin/pedidos':'#/cardapio'}">${admin?'Ver pedidos':'Fazer meu pedido'}</a></div><div class="panel-shortcuts"><a href="#/${admin?'admin':'cliente'}/pedidos">${P.icon('orders')}<strong>${admin?'Pedidos da loja':'Meus pedidos'}</strong><span>Ver andamento e detalhes</span></a><a href="#/${admin?'admin':'cliente'}/cupons">${P.icon('coupon')}<strong>Cupons</strong><span>${admin?'Gerenciar descontos':'Ver ofertas disponíveis'}</span></a><a href="#/conta/perfil">${P.icon('user')}<strong>Meus dados</strong><span>Dados pessoais e senha</span></a></div>`);
  };
  P.ordersPage=async (admin, query=new URLSearchParams()) => {
    if(!P.requireLogin(admin)) return;
    const page=Math.max(1,Number(query.get('pagina'))||1), status=query.get('status')||'';
    const params=new URLSearchParams({pagina:page}); if(admin&&status) params.set('status',status);
    const prefix=admin?'admin':'cliente';
    const data=await P.request('/api/'+prefix+'/pedidos?'+params);
    const rows=data.pedidos.map(order=>`<tr><td><strong>#${E(order.id.slice(0,8))}</strong><small>${P.date(order.criadoEm)}</small></td><td data-coluna="Cliente">${E(order.nomeCliente)}</td><td data-coluna="Andamento">${P.pill(order.status)}</td><td data-coluna="Recebimento">${E(order.recebimento)}</td><td data-coluna="Pagamento">${P.pill(order.pago?'Pago':'Pendente',order.pago?'is-success':'')}</td><td data-coluna="Total">${P.money(order.total)}</td><td><a class="btn btn-sm btn-outline-primary" href="#/${prefix}/pedidos/${order.id}">Ver detalhes</a></td></tr>`);
    const pageUrl=value=>`#/${prefix}/pedidos?${new URLSearchParams({pagina:value,...(status?{status}: {})})}`;
    P.panel(P.heading(admin?'Pedidos da loja':'Meus pedidos',`${data.total} pedido(s) encontrado(s).`)+(admin?`<form id="filtro-pedidos" class="panel-toolbar"><label>Status<select class="form-select" name="status"><option value="">Todos</option>${statuses.map(s=>`<option ${status===s?'selected':''}>${s}</option>`).join('')}</select></label><button class="btn btn-primary">Filtrar</button></form>`:'')+`<div class="panel-card">${rows.length?P.table(['Pedido','Cliente','Andamento','Recebimento','Pagamento','Total','Ações'],rows,'order-table'):P.empty('Nenhum pedido por aqui ainda.')}<nav class="panel-pagination" aria-label="Paginação de pedidos">${data.pagina>1?`<a href="${E(pageUrl(data.pagina-1))}">Anterior</a>`:''}<span>Página ${data.pagina} de ${data.totalPaginas}</span>${data.pagina<data.totalPaginas?`<a href="${E(pageUrl(data.pagina+1))}">Próxima</a>`:''}</nav></div>`,'pedidos');
    if(admin) document.getElementById('filtro-pedidos').onsubmit=event=>{event.preventDefault();location.hash='#/admin/pedidos?'+new URLSearchParams({status:new FormData(event.target).get('status')});};
  };
  P.orderPage=async (admin,id) => {
    if(!P.requireLogin(admin)) return;
    const result=await P.request('/api/'+(admin?'admin':'cliente')+'/pedidos/'+encodeURIComponent(id));
    const order=admin?result.pedido:result;
    const editor=admin?`<form id="pedido-status" class="panel-card form-grid mt-4"><h2>Atualizar pedido</h2><label>Andamento<select class="form-select" name="status">${statuses.map(s=>`<option ${order.status===s?'selected':''}>${s}</option>`).join('')}</select></label><label class="form-check"><input class="form-check-input" type="checkbox" name="pago" ${order.pago?'checked':''}> Pagamento recebido</label><div class="form-error" role="alert" hidden></div><button class="btn btn-primary">Salvar andamento</button></form><div class="panel-card mt-4"><h2>Histórico</h2><ul class="order-timeline">${result.movimentos.map(m=>`<li><strong>${E(m.statusAnterior)} → ${E(m.statusNovo)}</strong><span>Pagamento: ${m.pagoNovo?'recebido':'pendente'}</span><small>${P.date(m.criadoEm)} · ${E(m.administrador)}</small></li>`).join('')||'<li>Nenhuma atualização registrada.</li>'}</ul></div>`:`<button class="btn btn-primary mt-4" data-action="repetir" data-id="${order.id}">Pedir novamente</button>`;
    P.panel(P.heading('Pedido #'+order.id.slice(0,8),'Detalhes e andamento do pedido.',`<a href="#/${admin?'admin':'cliente'}/pedidos">Voltar aos pedidos</a>`)+P.orderDetail(order)+editor,'pedidos');
    if(admin) P.form(document.getElementById('pedido-status'),async (data,form)=>{
      await P.request('/api/admin/pedidos/'+id,{method:'PUT',body:{status:data.status,pago:form.elements.pago.checked}});
      P.message('Andamento atualizado.'); await P.orderPage(true,id);
    });
  };
  P.productsPage=async () => {
    if(!P.requireLogin(true)) return;
    const products=await P.request('/api/admin/produtos?removidos=true');
    const rows=products.map(p=>`<tr><td><div class="product-admin-name"><img src="${E(P.image(p.imagem))}" alt="" width="72" height="72"><span><strong>${E(p.nome)}</strong><small>${E(p.descricao)}</small></span></div></td><td>${E(p.categoria)}</td><td>${P.money(p.preco)}</td><td>${P.pill(p.removido?'Removido':p.ativo?'Disponível':'Indisponível',p.ativo?'is-success':'is-neutral')}</td><td><div class="table-actions">${p.removido?`<button class="btn btn-sm btn-outline-primary" data-action="restaurar-produto" data-id="${p.id}">Restaurar</button>`:`<a class="btn btn-sm btn-outline-primary" href="#/admin/produtos/${p.id}">Editar</a><button class="btn btn-sm btn-outline-danger" data-action="remover-produto" data-id="${p.id}">Remover</button>`}</div></td></tr>`);
    P.panel(P.heading('Cardápio','Gerencie os produtos e a disponibilidade.', '<a class="btn btn-primary" href="#/admin/produtos/novo">Novo produto</a>')+`<div class="panel-card">${P.table(['Produto','Categoria','Preço','Situação','Ações'],rows)}</div>`,'produtos');
  };
  P.productPage=async id => {
    if(!P.requireLogin(true)) return;
    const product=id==='novo'?{ativo:true,categoria:'Hambúrgueres'}:await P.request('/api/admin/produtos/'+encodeURIComponent(id));
    P.panel(P.heading(id==='novo'?'Novo produto':'Editar produto','Use uma imagem PNG, JPEG ou WebP de até 5 MB.')+`<form id="produto-form" class="panel-card form-grid">${P.fields([['nome','Nome','text','minlength="2" maxlength="120" required'],['descricao','Descrição','textarea','maxlength="500" required'],['categoria','Categoria','text','maxlength="40" required'],['preco','Preço (R$)','number','min="0.01" max="99999" step="0.01" required'],['arquivoImagem','Imagem','file',`accept="image/png,image/jpeg,image/webp" ${id==='novo'?'required':''}`],['ativo','Disponível no cardápio','checkbox']],product)}${product.imagem?`<img class="compact-image" src="${E(P.image(product.imagem))}" alt="Imagem atual">`:''}<div class="form-error" role="alert" hidden></div><div class="form-actions"><button class="btn btn-primary">Salvar produto</button><a class="btn btn-outline-primary" href="#/admin/produtos">Cancelar</a></div></form>`,'produtos');
    P.form(document.getElementById('produto-form'),async (_,form)=>{
      const data=new FormData(form); data.set('ativo',form.elements.ativo.checked?'true':'false');
      if(!form.elements.arquivoImagem.files.length) data.delete('arquivoImagem');
      await P.request('/api/admin/produtos'+(id==='novo'?'':'/'+id),{method:id==='novo'?'POST':'PUT',body:data});
      P.message('Produto salvo.'); location.hash='#/admin/produtos';
    });
  };
  P.couponsPage=async admin => {
    if(!P.requireLogin(admin)) return;
    const coupons=await P.request('/api/'+(admin?'admin':'cliente')+'/cupons');
    P.panel(P.heading(admin?'Cupons':'Meus cupons',admin?'Crie e acompanhe os descontos da loja.':'Aproveite os descontos no checkout.',admin?'<a class="btn btn-primary" href="#/admin/cupons/novo">Novo cupom</a>':'')+(coupons.length?`<div class="coupon-grid">${coupons.map(c=>`<article class="coupon-card"><h2>${c.tipo==='Percentual'?c.valor+'%':P.money(c.valor)} de desconto</h2><p class="coupon-code">${E(c.codigo)}</p><p>Compra mínima: ${P.money(c.compraMinima)}<br>${P.date(c.validoDe)} até ${P.date(c.validoAte)}</p>${admin?`<p>${c.usos} uso(s)${c.limiteUsos?' de '+c.limiteUsos:''} · ${c.ativo?'Ativo':'Inativo'}</p><div class="inline-actions"><a class="btn btn-sm btn-outline-primary" href="#/admin/cupons/${c.id}">Editar</a>${c.ativo?`<button class="btn btn-sm btn-outline-danger" data-action="desativar-cupom" data-id="${c.id}">Desativar</button>`:''}</div>`:c.limiteUsos!=null&&c.usos>=c.limiteUsos?P.pill('Esgotado','is-neutral'):c.umaVezPorCliente&&c.utilizadoPeloCliente?P.pill('Já utilizado','is-neutral'):`<button class="btn btn-outline-primary" data-action="copiar-cupom" data-codigo="${E(c.codigo)}">Copiar código</button>`}</article>`).join('')}</div>`:P.empty('Nenhum cupom disponível no momento.')),'cupons');
  };
  P.couponPage=async id => {
    if(!P.requireLogin(true)) return;
    const today=new Date().toLocaleDateString('en-CA');
    const coupon=id==='novo'?{tipo:'Percentual',valor:10,compraMinima:0,validoDe:today,validoAte:today,ativo:true,divulgar:true,umaVezPorCliente:true}:(await P.request('/api/admin/cupons')).find(c=>c.id===Number(id));
    if(!coupon) throw new Error('Cupom não encontrado.');
    P.panel(P.heading(id==='novo'?'Novo cupom':'Editar cupom')+`<form id="cupom-admin-form" class="panel-card form-grid">${P.fields([['codigo','Código','text','minlength="3" maxlength="24" pattern="[A-Za-z0-9_-]+" required']],coupon)}<label>Tipo de desconto<select name="tipo" class="form-select"><option value="Percentual" ${coupon.tipo==='Percentual'?'selected':''}>Percentual (%)</option><option value="ValorFixo" ${coupon.tipo==='ValorFixo'?'selected':''}>Valor fixo (R$)</option></select></label>${P.fields([['valor','Valor do desconto','number','min="0.01" max="99999" step="0.01" required'],['compraMinima','Compra mínima (R$)','number','min="0" max="99999" step="0.01" required'],['validoDe','Válido a partir de','date','required'],['validoAte','Válido até','date','required'],['limiteUsos','Limite total de usos (opcional)','number','min="1" max="1000000"'],['umaVezPorCliente','Uma utilização por cliente','checkbox'],['divulgar','Mostrar para os clientes','checkbox'],['ativo','Cupom ativo','checkbox']],coupon)}<div class="form-error" role="alert" hidden></div><div class="form-actions"><button class="btn btn-primary">Salvar cupom</button><a href="#/admin/cupons">Cancelar</a></div></form>`,'cupons');
    P.form(document.getElementById('cupom-admin-form'),async (data,form)=>{
      const body={...data,valor:Number(data.valor),compraMinima:Number(data.compraMinima),limiteUsos:data.limiteUsos?Number(data.limiteUsos):null};
      for(const key of ['ativo','divulgar','umaVezPorCliente']) body[key]=form.elements[key].checked;
      await P.request('/api/admin/cupons'+(id==='novo'?'':'/'+id),{method:id==='novo'?'POST':'PUT',body});
      P.message('Cupom salvo.'); location.hash='#/admin/cupons';
    });
  };
  P.clientsPage=async () => {
    if(!P.requireLogin(true)) return;
    const clients=await P.request('/api/admin/clientes');
    P.panel(P.heading('Clientes','Contas cadastradas e acesso à loja.')+`<div class="panel-card">${P.table(['Cliente','Contato','Pedidos','Acesso','Ações'],clients.map(c=>`<tr><td><strong>${E(c.nome)}</strong><small>Desde ${P.date(c.criadoEm)}</small></td><td>${E(c.email)}<small>${E(c.telefone)}</small></td><td>${c.pedidos}</td><td>${P.pill(c.ativo?'Ativo':'Bloqueado',c.ativo?'is-success':'is-danger')}</td><td><button class="btn btn-sm btn-outline-primary" data-action="acesso-cliente" data-id="${c.id}" data-ativo="${!c.ativo}">${c.ativo?'Bloquear':'Liberar'} acesso</button></td></tr>`))}</div>`,'clientes');
  };
  P.reportsPage=async (query=new URLSearchParams()) => {
    if(!P.requireLogin(true)) return;
    const params=new URLSearchParams(); for(const key of ['inicio','fim']) if(query.get(key)) params.set(key,query.get(key));
    const report=await P.request('/api/admin/relatorios?'+params);
    const max=Math.max(1,...report.dias.map(d=>d.valor));
    P.panel(P.heading('Relatórios','Vendas e andamento dos pedidos no período.')+`<form id="relatorio-form" class="panel-toolbar"><label>Início<input class="form-control" name="inicio" type="date" value="${report.inicio}" required></label><label>Fim<input class="form-control" name="fim" type="date" value="${report.fim}" required></label><button class="btn btn-primary">Consultar</button></form>`+P.metrics([['Pedidos',report.totalPedidos],['Valor dos pedidos',P.money(report.valorPedidos)],['Recebido',P.money(report.valorRecebido)],['Ticket médio',P.money(report.ticketMedio)]])+`<div class="row g-4"><div class="col-lg-8"><section class="panel-card"><h2>Vendas por dia</h2><div class="report-chart">${report.dias.map(d=>`<div class="report-bar"><strong>${P.date(d.data)}</strong><span style="width:${Math.max(1,d.valor/max*100)}%" role="img" aria-label="${d.pedidos} pedidos"></span><strong>${P.money(d.valor)}</strong></div>`).join('')||'<p>Nenhuma venda no período.</p>'}</div></section></div><div class="col-lg-4"><section class="panel-card"><h2>Andamento</h2><dl class="status-counts">${report.status.map(s=>`<div><dt>${E(s.nome)}</dt><dd>${s.quantidade}</dd></div>`).join('')}</dl><p>Cancelados: ${report.pedidosCancelados}<br>Pedidos pagos: ${report.pedidosPagos}<br>Descontos: ${P.money(report.descontos)}</p></section></div></div><section class="panel-card mt-4"><h2>Produtos vendidos</h2>${P.table(['Produto','Quantidade','Valor'],report.produtos.map(p=>`<tr><td>${E(p.nome)}</td><td>${p.quantidade}</td><td>${P.money(p.valor)}</td></tr>`))}</section>`,'relatorios');
    document.getElementById('relatorio-form').onsubmit=event=>{event.preventDefault();location.hash='#/admin/relatorios?'+new URLSearchParams(new FormData(event.target));};
  };
  P.action=async button => {
    const id=button.dataset.id, action=button.dataset.action;
    if(action==='sair') return P.logout();
    if(action==='repetir') {
      const result=await P.request('/api/cliente/pedidos/'+id+'/repetir',{method:'POST'});
      if(!result.itens.length) throw new Error('Os produtos desse pedido não estão disponíveis.');
      if(P.cart.length&&!confirm('Substituir os itens atuais do carrinho por este pedido?')) return;
      // Valida os limites antes de substituir o carrinho.
      if(result.itens.length>30||result.itens.some(x=>x.quantidade>20)||result.itens.reduce((s,x)=>s+x.quantidade,0)>100) throw new Error('Esse pedido ultrapassa os limites atuais do carrinho.');
      P.cart=result.itens; P.persistCart(); location.hash='#/carrinho';
      if(result.indisponiveis.length) P.message('Itens indisponíveis não foram adicionados: '+result.indisponiveis.join(', '),'warning');
      return;
    }
    if(action==='copiar-cupom') {
      try { await navigator.clipboard.writeText(button.dataset.codigo); P.message('Código copiado: '+button.dataset.codigo); }
      catch { P.message('Use este código no checkout: '+button.dataset.codigo,'info'); }
      return;
    }
    if(!P.requireLogin(true)) return;
    if(action==='remover-produto') {
      if(!confirm('Remover este produto do cardápio? Pedidos existentes serão preservados.')) return;
      await P.request('/api/admin/produtos/'+id,{method:'DELETE'}); P.message('Produto removido.'); return P.productsPage();
    }
    if(action==='restaurar-produto') {
      await P.request('/api/admin/produtos/'+id+'/restaurar',{method:'POST'}); P.message('Produto restaurado. Edite para disponibilizá-lo.'); return P.productsPage();
    }
    if(action==='desativar-cupom') {
      if(!confirm('Desativar este cupom?')) return;
      await P.request('/api/admin/cupons/'+id+'/desativar',{method:'POST'}); P.message('Cupom desativado.'); return P.couponsPage(true);
    }
    if(action==='acesso-cliente') {
      if(!confirm(button.dataset.ativo==='true'?'Liberar o acesso deste cliente?':'Bloquear o acesso deste cliente e encerrar suas sessões?')) return;
      await P.request('/api/admin/clientes/'+id+'/acesso',{method:'PUT',body:button.dataset.ativo==='true'}); P.message('Acesso atualizado.'); return P.clientsPage();
    }
  };
})();
