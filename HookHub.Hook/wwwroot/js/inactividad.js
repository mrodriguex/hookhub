var tiempoInactividad = 150;    //300;    //tiempo que espera en inactividad
var tiempoParaMostrar = 60;     //tiempo que permanece visible el popup

var countdownTimer;
var countdownTimer2;

var segundosFinales = tiempoParaMostrar;
var contador = 0;

var modalAbierto;

$(document).ready(function () {
    contadorGeneral();
});

$(document).on('mousemove', function () {
    clearTimeout(countdownTimer);
    contadorGeneral();
});

$(document).on('shown.bs.modal', function (e) {
    modalAbierto = $(e.target).attr('id');
});

function contadorGeneral() {
    countdownTimer = setTimeout(mostrarAlerta, tiempoInactividad * 1000);
}

function mostrarAlerta() {
    contadorFinal();
    if ($('body').hasClass('modal-open')) {
        modalAbierto = '#' + modalAbierto;
        //$(modalAbierto).modal('hide');
    }
    $('#modalInactividad').modal('show');
}

function contadorFinal() {
    $(document).off("mousemove");
    segundosFinales = tiempoParaMostrar;
    contador = 0;
    countdownTimer2 = setInterval(mostrarCuentaRegresiva, 1000);
}

function mostrarCuentaRegresiva() {
    var minutes = Math.floor((segundosFinales) / 60);
    var remainingSeconds = segundosFinales % 60;

    segundos = ("0" + remainingSeconds).slice(-2);
    document.getElementById('tiempo').innerHTML = minutes + ":" + segundos;

    if (segundosFinales > 0) {
        contador = contador + 1;
        var newprogress = (contador / tiempoParaMostrar) * 430;
        $('#progressbar').attr('aria-valuenow', newprogress).css('width', newprogress);
        segundosFinales--;
    } else {
        clearInterval(countdownTimer2);
        var rutaLogout = $("#rutaLogout").val();
        window.location.replace(rutaLogout);
    }
}

function cerrarModalInactividad() {
    $(document).on('mousemove', function () {
        clearInterval(countdownTimer);
        //seconds = tiempoInactividad;
        segundosFinales = tiempoParaMostrar;
        contadorGeneral();
    });
    contador = 1;
    clearInterval(countdownTimer2);
    $('#modalInactividad').modal('hide');
}




