$(function () {
    var $t = function (selector) {
        return $("#JtuyoshiCrop").find(selector)
    };
    var SelectMudar_Orientacao = function () {
        var CW, CH, CX, CY;
        var princWidth = $t("#Principal").width();
        var princHeight = $t("#Principal").height();
        if ($t("#SelectOrientacao").val() == "Horizontal") {
            var aspectRatioOption = $t("#SelectProporcao").val();
            if (aspectRatioOption == "box") {
                CH = ((princHeight / 100) * 80);
                CW = (CH * 4) / 3;
                if (CW > princWidth) {
                    CW = ((princWidth / 100) * 80);
                    CH = (CW / 4) * 3
                }
                CX = ((princWidth - CW) / 2);
                CY = ((princHeight - CH) / 2);
                $t("#SelecaoRecorte").stop().animate({
                    "left": CX,
                    "top": CY,
                    "width": CW,
                    "height": CH
                }, 300);
                $t("#SelecaoRecorte").resizable("destroy").resizable({
                    containment: "parent",
                    aspectRatio: 4 / 3,
                    minWidth: 100,
                    minHeight: 100
                })
            } else if (aspectRatioOption == "wide") {
                if (princWidth < princHeight) {
                    CW = (princWidth / 100) * 80;
                    CH = (CW / 16) * 9;
                } else {
                    CH = (princHeight / 100) * 80;
                    CW = (CH * 16) / 9;
                };
                CX = ((princWidth - CW) / 2);
                CY = ((princHeight - CH) / 2);
                $t("#SelecaoRecorte").stop().animate({
                    "left": CX,
                    "top": CY,
                    "width": CW,
                    "height": CH
                }, 300);
                $t("#SelecaoRecorte").resizable("destroy").resizable({
                    containment: "parent",
                    aspectRatio: 16 / 9,
                    minWidth: 100,
                    minHeight: 100
                })
            } else if (aspectRatioOption == "square") {
                if (princWidth < princHeight) {
                    CW = (princWidth / 100) * 80;
                    CH = CW; 
                } else {
                    CH = (princHeight / 100) * 80;
                    CW = CH;
                };
                CX = ((princWidth - CW) / 2);
                CY = ((princHeight - CH) / 2);
                $t("#SelecaoRecorte").stop().animate({
                    "left": CX,
                    "top": CY,
                    "width": CW,
                    "height": CH
                }, 300);
                $t("#SelecaoRecorte").resizable("destroy").resizable({
                    containment: "parent",
                    aspectRatio: 1,
                    minWidth: 50,
                    minHeight: 50
                })
            } else {
                if (princWidth < princHeight) {
                    CW = (princWidth / 100) * 80;
                    CH = CW;
                } else {
                    CH = ((princHeight / 100) * 80);
                    CW = CH;
                };
                CX = ((princWidth - CW) / 2);
                CY = ((princHeight - CH) / 2)
                $t("#SelecaoRecorte").stop().animate({
                    "left": CX,
                    "top": CY,
                    "width": CW,
                    "height": CH
                }, 300).resizable("destroy").resizable({
                    containment: "parent",
                    minWidth: 100,
                    minHeight: 100
                })
            }
        } else {
            if ($t("#SelectProporcao").val() == "box") {
                CH = ((princHeight / 100) * 60);
                CW = (CH * 3) / 4;
                CX = ((princWidth - CW) / 2);
                CY = ((princHeight - CH) / 2);
                $t("#SelecaoRecorte").stop().animate({
                    "left": CX,
                    "top": CY,
                    "width": CW,
                    "height": CH
                }, 300).resizable("destroy").resizable({
                    containment: "parent",
                    aspectRatio: 3 / 4,
                    minWidth: 100,
                    minHeight: 100
                });
                $t("#SelecaoRecorte").resizable("option", "aspectRatio", 3 / 4)
            } else if ($t("#SelectProporcao").val() == "wide") {
                CH = ((princHeight / 100) * 80);
                CW = (CH * 9) / 16;
                CX = ((princWidth - CW) / 2);
                CY = ((princHeight - CH) / 2);
                $t("#SelecaoRecorte").stop().animate({
                    "left": CX,
                    "top": CY,
                    "width": CW,
                    "height": CH
                }, 300).resizable("destroy").resizable({
                    containment: "parent",
                    aspectRatio: 9 / 16,
                    minWidth: 100,
                    minHeight: 100
                })
            } else {
                if (princWidth < princHeight) {
                    CW = (princWidth / 100) * 80;
                    CH = CW;
                    CX = ((princWidth - CW) / 2);
                    CY = ((princHeight - CH) / 2)
                } else {
                    CH = ((princHeight / 100) * 80);
                    CW = CH;
                    CX = ((princWidth - CW) / 2);
                    CY = ((princHeight - CH) / 2)
                };
                $t("#SelecaoRecorte").stop().animate({
                    "left": CX,
                    "top": CY,
                    "width": CW,
                    "height": CH
                }, 300).resizable("destroy").resizable({
                    containment: "parent",
                    minWidth: 100,
                    minHeight: 100
                })
            }
        }
    };
    $t("#SelectOrientacao, #SelectProporcao").change(function () {
        SelectMudar_Orientacao()
    })
});