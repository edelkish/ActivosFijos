function mostrarLoadingPanel(idElemento, texto) {
    $('#' + idElemento).waitMe({
        effect: 'roundBounce',
        text: texto != null ? texto : 'Procesando datos, por favor espere...',
        bg: 'rgba(255, 255, 255, 0.7)',
        color: '#ef4c0c'
    });
}

function mostrarLoadingPanelClass(classElemento, texto) {
    $('.' + classElemento).waitMe({
        effect: 'roundBounce',
        text: texto != null ? texto : 'Procesando datos, por favor espere...',
        bg: 'rgba(255, 255, 255, 0.7)',
        color: '#ef4c0c'
    });
}

function ocultarLoadingPanelClass(classElemento) {
    $("." + classElemento).removeClass("waitMe_container");
    $(".waitMe").remove();
}

function ocultarLoadingPanel(idElemento) {
    $("#" + idElemento).removeClass("waitMe_container");
    $(".waitMe").remove();
}