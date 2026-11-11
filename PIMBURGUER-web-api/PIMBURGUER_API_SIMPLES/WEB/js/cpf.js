(() => {
    const digits = value => value.replace(/[^0-9]/g, '');
    const valid = value => {
        const cpf = digits(value);
        if (cpf.length !== 11 || /^(.)\1{10}$/.test(cpf)) return false;
        for (let size = 9; size <= 10; size++) {
            let sum = 0;
            for (let i = 0; i < size; i++) sum += Number(cpf[i]) * (size + 1 - i);
            const remainder = sum % 11;
            if ((remainder < 2 ? 0 : 11 - remainder) !== Number(cpf[size])) return false;
        }
        return true;
    };
    const forms = new Map();
    for (const [index, field] of [...document.querySelectorAll('[data-cpf]')].entries()) {
        const form = field.form;
        let feedback = [...(form?.querySelectorAll('[data-valmsg-for]') ?? [])]
            .find(element => element.getAttribute('data-valmsg-for') === field.name);
        if (!feedback) {
            feedback = document.createElement('span');
            feedback.className = 'field-error field-validation-valid';
            field.insertAdjacentElement('afterend', feedback);
        }
        feedback.id ||= `${field.id || `cpf-${index}`}-erro`;
        feedback.classList.add('cpf-feedback');
        feedback.setAttribute('aria-live', 'polite');
        feedback.setAttribute('aria-atomic', 'true');
        const descriptions = new Set((field.getAttribute('aria-describedby') || '').split(/\s+/).filter(Boolean));
        descriptions.add(feedback.id);
        field.setAttribute('aria-describedby', [...descriptions].join(' '));
        let touched = feedback.classList.contains('field-validation-error');

        const update = () => {
            const caret = field.selectionStart ?? field.value.length;
            const before = digits(field.value.slice(0, caret)).length;
            const cpf = digits(field.value).slice(0, 11);
            let formatted = '';
            for (let i = 0; i < cpf.length; i++) {
                if (i === 3 || i === 6) formatted += '.';
                if (i === 9) formatted += '-';
                formatted += cpf[i];
            }
            field.value = formatted;
            let position = 0, count = 0;
            while (position < formatted.length && count < before) {
                if (/[0-9]/.test(formatted[position])) count++;
                position++;
            }
            if (document.activeElement === field) field.setSelectionRange(position, position);
            const message = !cpf
                ? (field.required ? 'Informe o CPF.' : '')
                : cpf.length !== 11 ? 'Informe os 11 dígitos do CPF.'
                : valid(cpf) ? '' : 'Informe um CPF válido.';
            field.setCustomValidity(message);
            // Mostra o erro ao completar os dígitos, sair do campo ou tentar enviar.
            const showError = Boolean(message && (touched || cpf.length === 11));
            feedback.textContent = showError ? message : '';
            feedback.classList.toggle('field-validation-error', showError);
            feedback.classList.toggle('field-validation-valid', !showError);
            field.classList.toggle('is-invalid', showError);
            field.classList.toggle('input-validation-error', showError);
            field.setAttribute('aria-invalid', String(showError));
            return !message;
        };
        field.addEventListener('input', update);
        const reveal = () => { touched = true; return update(); };
        field.addEventListener('blur', reveal);
        field.addEventListener('change', reveal);
        field.addEventListener('invalid', reveal);
        window.addEventListener('pageshow', update);
        if (form) {
            if (!forms.has(form)) forms.set(form, []);
            forms.get(form).push({ field, update, reveal });
        }
        update();
    }
    for (const [form, controls] of forms) {
        // Atualiza também valores preenchidos automaticamente, antes da validação nativa.
        form.addEventListener('click', event => {
            const submit = event.target.closest('button, input');
            if (submit?.type === 'submit' && !submit.formNoValidate)
                for (const control of controls) control.reveal();
        }, true);
        form.addEventListener('submit', event => {
            if (event.submitter?.formNoValidate) return;
            const invalid = controls.filter(control => !control.reveal());
            if (invalid.length) {
                event.preventDefault();
                invalid[0].field.focus();
                invalid[0].field.reportValidity();
            }
        }, true);
    }
})();
