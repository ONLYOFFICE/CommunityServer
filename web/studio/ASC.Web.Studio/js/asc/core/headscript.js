function setCustomVh() {
    var vh = window.innerHeight * 0.01;
    document.documentElement.style.setProperty('--vh', `${vh}px`);
}

function setContentFocus(e) {
    var keyCodes = [33, 34, 38, 40];
    if (keyCodes.indexOf(e.keyCode) == -1) return;
    var content = document.querySelector('#studioPageContent .mainPageContent')
    if (e.target == content) return;
    if (e.target instanceof HTMLTextAreaElement) return;
    if (e.target instanceof HTMLInputElement) {
        var inputTypes = ['text', 'password', 'number', 'email'];
        if (inputTypes.indexOf(e.target.type) >= 0) return;
    }
    if (jq && jq('.popupContainerClass:visible, .advanced-selector-container:visible').length) return;
    content.focus();
}

window.addEventListener('resize', setCustomVh);

window.addEventListener('keydown', setContentFocus);

setCustomVh();