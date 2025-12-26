// Opera 8.0+
var isOpera = (!!window.opr && !!opr.addons) || !!window.opera || navigator.userAgent.indexOf(' OPR/') >= 0;

// Firefox 1.0+
var isFirefox = typeof InstallTrigger !== 'undefined';

// Safari 3.0+ "[object HTMLElementConstructor]" 
var isSafari = /constructor/i.test(window.HTMLElement) || (function (p) { return p.toString() === "[object SafariRemoteNotification]"; })(!window['safari'] || (typeof safari !== 'undefined' && safari.pushNotification));

// Internet Explorer 6-11
var isIE = /*@cc_on!@*/false || !!document.documentMode;

// Edge 20+
var isEdge = !isIE && !!window.StyleMedia;

// Chrome 1 - 79
var isChrome = !!window.chrome && (!!window.chrome.webstore || !!window.chrome.runtime);

// Edge (based on chromium) detection
var isEdgeChromium = isChrome && (navigator.userAgent.indexOf("Edg") != -1);

// Blink engine detection
var isBlink = (isChrome || isOpera) && !!window.CSS;

/////////////////////////////////////////////////////////////////////////////////////
/// Para exportar un elemento html a EXCEL //////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////////////

var tableToExcel = (function () {
    var uri = 'data:application/vnd.ms-excel;base64,'
        , template = '<html xmlns:o="urn:schemas-microsoft-com:office:office" xmlns:x="urn:schemas-microsoft-com:office:excel" xmlns="http://www.w3.org/TR/REC-html40"><head><!--[if gte mso 9]><xml><x:ExcelWorkbook><x:ExcelWorksheets><x:ExcelWorksheet><x:Name>{worksheet}</x:Name><x:WorksheetOptions><x:DisplayGridlines/></x:WorksheetOptions></x:ExcelWorksheet></x:ExcelWorksheets></x:ExcelWorkbook></xml><![endif]--><meta http-equiv="content-type" content="text/plain; charset=UTF-8"/></head><body><table>{table}</table></body></html>'
        , base64 = function (s) { return window.btoa(unescape(encodeURIComponent(s))) }
        , format = function (s, c) { return s.replace(/{(\w+)}/g, function (m, p) { return c[p]; }) }
    return function (table, name) {
        if (!table.nodeType) table = document.getElementById(table)

        var tableInnerHtml = isIE || isEdge ? innerHtmlCssInline : table.innerHTML;

        var ctx = { worksheet: name || 'Worksheet', table: tableInnerHtml }

        if (isIE || isEdge) {
            tab_text = [format(template, ctx)];
            var blob1 = new Blob(tab_text, {
                type: "text/html"
            });
            window.navigator.msSaveBlob(blob1, "download.xls");
        }
        else {
            window.location.href = uri + base64(format(template, ctx))
        }
    }
})();

var tablesToExcel = (function () {
    var uri = 'data:application/vnd.ms-excel;base64,'
        , template = '<html xmlns:o="urn:schemas-microsoft-com:office:office" xmlns:x="urn:schemas-microsoft-com:office:excel" xmlns="http://www.w3.org/TR/REC-html40"><head><!--[if gte mso 9]><xml><x:ExcelWorkbook><x:ExcelWorksheets><x:ExcelWorksheet><x:Name>{worksheet}</x:Name><x:WorksheetOptions><x:DisplayGridlines/></x:WorksheetOptions></x:ExcelWorksheet></x:ExcelWorksheets></x:ExcelWorkbook></xml><![endif]--><meta http-equiv="content-type" content="text/plain; charset=UTF-8"/></head><body>{tables}</body></html>'
        , base64 = function (s) { return window.btoa(unescape(encodeURIComponent(s))) }
        , format = function (s, c) { return s.replace(/{(\w+)}/g, function (m, p) { return c[p]; }) }
    return function (tables, name) {
        var tablesInnerHtml = '';
        for (var i = 0; i < tables.length; i++) {
            var table = tables[i];
            if (!table.nodeType) table = document.getElementById(table)

            tablesInnerHtml += '<table>' + (isIE || isEdge ? innerHtmlCssInline : table.innerHTML) + '</table>';
        }

        var ctx = { worksheet: name || 'Worksheet', tables: tablesInnerHtml }

        if (isIE || isEdge) {
            tab_text = [format(template, ctx)];
            var blob1 = new Blob(tab_text, {
                type: "text/html"
            });
            window.navigator.msSaveBlob(blob1, "download.xls");
        }
        else {
            window.location.href = uri + base64(format(template, ctx))
        }
    }
})();

var estilosAceptados = [
    'align-content',
    'align-items',
    'background-color',
    'border-top-color',
    'border-top-style',
    'border-top-width',
    'border-left-color',
    'border-left-style',
    'border-left-width',
    'border-right-color',
    'border-right-style',
    'border-right-width',
    'border-bottom-color',
    'border-bottom-style',
    'border-bottom-width',
    'color',
    'font-family',
    'font-size',
    'font-weight',
    'height',
    'max-height',
    'max-width',
    'min-height',
    'min-width',
    'padding',
    'padding-top',
    'padding-left',
    'padding-right',
    'padding-bottom',
    'text-align',
    'width',
    'vertical-align'];

getCurrentStyle = function (el) {
    if (el.currentStyle)
        return el.currentStyle;
    else if (window.getComputedStyle)
        return document.defaultView.getComputedStyle(el, null);
    return null;
}

function applyStyle(el) {
    s = getCurrentStyle(el);    //getComputedStyle(el);
    var styles = {};
    for (var i = 0; i < estilosAceptados.length; i++) {
        var key = estilosAceptados[i];
        var valor = $(el).css(key);  //s[key];
        if (valor !== "" && el.style[key] === "") {
            //el.style[key] = valor;
            styles[key] = valor;
        }
    }
    return (styles);
    //el.style = styles;
}

isEmpty = function (x, p) { for (p in x) return !1; return !0 };

(function (jQuery) {
    jQuery.extend(jQuery.fn, {
        makeCssInline: function (sizes) {
            this.each(function (idx, el) {
                var styles = applyStyle(this);
                jQuery(this).children().makeCssInline();
                for (const property in styles) {
                    this.style[property] = styles[property];
                }
            });
        }
    });
}(jQuery));

var innerHtmlCssInline = '';

function makeCssInlineStr(el) {
    var outerEl = $(el).clone().empty();
    var outerHtmlCssInlineConcat = outerEl.length ? outerEl[0].outerHTML : '';
    //var innerHtmlCssInlineConcat = el.innerHTML;
    //var tagCierre = el.children.length > 0 ? '</' + el.tagName + '>' : '';
    //innerHtmlCssInlineConcat = innerHtmlCssInlineConcat.replace(tagCierre, '');

    var styles = applyStyle(el);
    var stylesStr = "";
    var regClass = /(class=".*?")/;
    for (const property in styles) {
        if (!(property === 'max-width' && styles[property] === 'none')) {
            stylesStr += property + ':' + styles[property] + ';';
        }
        //this.style[property] = styles[property];
    }

    if (stylesStr !== "") {
        var regStyle0 = /(style='.*?')/;
        var regStyle1 = /(style=".*?")/;
        outerHtmlCssInlineConcat = outerHtmlCssInlineConcat.replace(regStyle0, '');
        outerHtmlCssInlineConcat = outerHtmlCssInlineConcat.replace(regStyle1, '');
        outerHtmlCssInlineConcat = outerHtmlCssInlineConcat.replace(regClass, " style='" + stylesStr + "' ");
    }

    var innerHtmlCssInlineConcat = '';
    for (var i = 0; i < el.children.length; i++) {
        var element = el.children[i];
        innerHtmlCssInlineConcat += '' + makeCssInlineStr(element);
    }

    if (el.children.length) {
        return (outerHtmlCssInlineConcat.replace("></", ">" + innerHtmlCssInlineConcat + "</"));
    }
    else {        
        return (outerHtmlCssInlineConcat.replace("></", ">" + el.innerHTML + "</"));
    }
}
/////////////////////////////////////////////////////////////////////////////////////

$(document).on('show.bs.modal', '.modal', function (event) {
    var zIndex = 1040 + (10 * $('.modal:visible').length);
    $(this).css('z-index', zIndex);
    setTimeout(function () {
        $('.modal-backdrop').not('.modal-stack').css('z-index', zIndex - 1).addClass('modal-stack');
    }, 0);
});

function toJSONString(form) {
    var obj = {};
    var elements = form.querySelectorAll("input, select, textarea");
    for (var i = 0; i < elements.length; ++i) {
        var element = elements[i];
        var name = element.name;
        var value = element.value;

        if (name) {
            obj[name] = value;
        }
    }
    return obj;
}

function gridDataToJSON(object) {
    var obj = {};
    var propName;
    var element;
    var value;

    if (Array.isArray(object)) {
        obj = [{}];
        var obji = {};
        for (var i = 0; i < object.length; ++i) {
            element = object[i];
            for (propName in element) {
                if (element.hasOwnProperty(propName)) {
                    if (propName !== null) {
                        value = element[propName];
                        obji[propName] = typeof propName === "object" ? gridDataToJSON(value) : value;
                        obj.push(obji);
                    }
                }
            }
        }
    }
    else {
        element = object;
        for (propName in element) {
            if (element.hasOwnProperty(propName)) {
                if (propName !== null) {
                    value = element[propName];
                    obj[propName] = typeof propName === "object" ? gridDataToJSON(value) : value;
                }
            }
        }
    }
    return obj;
}

function noback() {
    window.location.hash = "NB";
    window.location.hash = "Again-No-back-button"; //chrome
    window.onhashchange = function () { window.location.hash = ""; };
}


/////////////////////////////////////////////////////////////////////////////////////
/// Para exportar un elemento html a PDF ////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////////////
function htmlToPdf(divSelector, pdfName, landscape) {
    var draw = kendo.drawing;

    kendo.drawing.drawDOM(divSelector, { scale: 0.51 })
        .then(function (root) {
            // Chaining the promise via then
            return draw.exportPDF(root, {
                paperSize: "letter",
                margin: {
                    left: "10mm",
                    top: "10mm",
                    right: "10mm",
                    bottom: "10mm"
                },
                landscape: landscape
            });
        })
        .done(function (data) {
            // Here 'data' is the Base64-encoded PDF file
            kendo.saveAs({
                dataURI: data,
                fileName: pdfName
            });
        });
    //kendo.drawing.drawDOM(divSelector, {
    //    paperSize: "letter", margin: {
    //        left: "0mm",
    //        top: "0mm",
    //        right: "0mm",
    //        bottom: "0mm"
    //    },
    //    scale: 0.5
    //}).then(function (group) {
    //    kendo.drawing.pdf.saveAs(group, pdfName);
    //});
}
/////////////////////////////////////////////////////////////////////////////////////


/////////////////////////////////////////////////////////////////////////////////////
/// Para que funcionen los filtros en el DropDownList dentro del Grid en IE11 ///////
/////////////////////////////////////////////////////////////////////////////////////
function quitarFoco() {
    $(document).off('focusin.bs.modal');
}
/////////////////////////////////////////////////////////////////////////////////////

function FiltroFormatoEntero(e) {
    e.element.kendoNumericTextBox({
        spinners: false,
        format: "#",
        decimals: 0
    });
}

function FiltroFormatoDecimal(e) {
    e.element.kendoNumericTextBox({
        spinners: false,
        format: "#",
        decimals: 2
    });

}

function CreateEstatusTemplate(data) {
    var template = "<input type='image' src='images/" + (data.Estatus !== true ? "Inactivo" : "Activo") + ".png' />";
    return template;
}

function boolDropDown(args) {

    args.element.kendoDropDownList({

        dataSource: [{ value: true, text: "On", liga: "images/Activo.png" }, { value: false, text: "Off", liga: "images/Inactivo.png" }],

        optionLabel: "Filtro",

        dataTextField: "text",

        dataValueField: "value",
        template: '<center> <img src="#:data.liga#"/>  </center>'

    });

}

function onFilter(e) {

    if (e.field === "Estatus") {

        if (e.filter && e.filter.filters && e.filter.filters.length > 0) {

            e.filter.filters[0].value = (e.filter.filters[0].value === "true");


        }

    }
}

