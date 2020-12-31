$(document).ready(function () {
    var result = 0;
    var prevEntry = 0;
    var operation = null;
    var currentEntry = '0';
    updateScreen(result);
    $('.calculator').find('.button').each(function () {
        $('.button').attr("data-Status", "Ideal");
        $('.button').css('color', 'black');
        $('.button').css('background-color', 'white');
    });

});
$('.button').on('click', function (evt) {
    var buttonPressed = $(this).html();
    currentEntry = buttonPressed;
    
    if ($(this).html() == "9") {
        $('.calculator').find('.button').each(function () {
            $('.button').attr("data-Status", "Off");
            $('.button').css('background-color', 'red');
        });
        $('.zero').css('background-color', 'white');
        $('.nine').css('background-color', 'white');
       
        CallApi("9", "Off")
    }

    if ($(this).html() == "0") {
        $('.calculator').find('.button').each(function () {
            $('.button').attr("data-Status", "On");
            $('.button').css('background-color', 'green');
        });
        $('.zero').css('background-color', 'white');
        $('.nine').css('background-color', 'white');
      
        CallApi("0", "On")
    }
    //$('.calculator').find('.button').not($(this)).each(function () {
    //    $('.button').attr("data-Status", "Ideal");
    //    $('.button').css('color', 'black');
    //});
    if ($(this).html() != "9" && $(this).html() != "0") {
        console.log($(this).html());
        if ($(this).attr("data-Status") == "On") {

            $(this).attr("data-Status", "Off");
            $(this).css('background-color', 'red');

        }
        else if ($(this).attr("data-Status") == "Off") {
            $(this).attr("data-Status", "On");
            $(this).css('background-color', 'green');
        }
        else {
           
            $(this).attr("data-Status", "On");
            $(this).css('background-color', 'green');
        }

        CallApi($(this).html(), $(this).attr("data-Status"))
    }



  
    //9 all off 0 all on 
  
    $('#bt9').attr("data-Status", "Ideal");
    $('#bt9').css('color', 'black');
    $('#bt0').attr("data-Status", "Ideal");
    $('#bt0').css('color', 'black');
    //if (buttonPressed === "C") {
    //    result = 0;
    //    currentEntry = '0';
    //} else if (buttonPressed === "CE") {
    //    currentEntry = '0';
    //} else if (buttonPressed === "back") {
    //    //currentEntry = currentEntry.substring(0, currentEntry.length-1);
    //} else if (buttonPressed === "+/-") {
    //    currentEntry *= -1;
    //} else if (buttonPressed === '.') {
    //    currentEntry += '.';
    //} else if (isNumber(buttonPressed)) {
    //    if (currentEntry === '0') currentEntry = buttonPressed;
    //    else currentEntry = currentEntry + buttonPressed;
    //} else if (isOperator(buttonPressed)) {
    //    prevEntry = parseFloat(currentEntry);
    //    operation = buttonPressed;
    //    currentEntry = '';
    //} else if (buttonPressed === '%') {
    //    currentEntry = currentEntry / 100;
    //} else if (buttonPressed === 'sqrt') {
    //    currentEntry = Math.sqrt(currentEntry);
    //} else if (buttonPressed === '1/x') {
    //    currentEntry = 1 / currentEntry;
    //} else if (buttonPressed === 'pi') {
    //    currentEntry = Math.PI;
    //} else if (buttonPressed === '=') {
    //    currentEntry = operate(prevEntry, currentEntry, operation);
    //    operation = null;
    //}

    updateScreen(currentEntry);

});
updateScreen = function (displayValue) {
    var displayValue = displayValue.toString();
    $('.screen').html(displayValue.substring(0, 10));
};
CallApi = function (processId, processStatus) {
    var ProcessDto = new Object();
    ProcessDto.Id = processId;
    ProcessDto.Status = processStatus;
    console.log(JSON.stringify(ProcessDto));
    $.ajax({
        url: 'http://localhost:5001/api/Process',
        type: 'POST',
        crossDomain: true,
        dataType: 'json',
        async: false,
        contentType: "application/json;",
        data: JSON.stringify( ProcessDto),
        success: function (data, textStatus, xhr) {
            console.log(data);
        },
        error: function (xhr, textStatus, errorThrown) {
            console.log('Error in Operation');
            console.log(errorThrown);
        }
    });
};

isNumber = function (value) {
    return !isNaN(value);
}

isOperator = function (value) {
    return value === '/' || value === '*' || value === '+' || value === '-';
};

operate = function (a, b, operation) {
    a = parseFloat(a);
    b = parseFloat(b);
    console.log(a, b, operation);
    if (operation === '+') return a + b;
    if (operation === '-') return a - b;
    if (operation === '*') return a * b;
    if (operation === '/') return a / b;
}
//https://codepen.io/simonja2/pen/QbGYbR?__cf_chl_jschl_tk__=0444f1034e55f98d9d34bb875ff1ed10d372d8f9-1605554904-0-AZWlIuT5nTNHnIC-c_XWanaKTFofxLpoWQIh1_6EfGlxb6Rc327HFZjEyudnGsHeBgCibGXOZooVJkoMJIg4xfqxVP733vOT2-bc8NEmIEH82htBbETTHdmeHhAURvjjVIuJf9srvtNGtKQxdXlOzrEqVKOZWOwXXPuFCqgZpjKiwk6f0s-C7-GslTCAxz4L_PvdSUPRwC1vZZsjMugpah5HmaWmnt6V1dtSEge0zVZXoySgf3jKSywLeZqbebHOylIo7QfpMFMNfRFmDSUjWCHWILSDq9gASyqtd34HtTGUdq2fK-tuNRIs87XKdJYPFgciAWQu0DS8UihU8_xJQW2yfeUNdxY9B3sgFu0hqwWt-3EjuPJBZq_6GI8rqZzhAsTCBHrAj67vFpWqcYz_L3A