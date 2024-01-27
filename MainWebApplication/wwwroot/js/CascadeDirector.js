$(document).ready(function () {
    $('#CarId').change(function () {
        var id = $(this).val();
        $('#ModelAutoId').empty();
        $('#ModelAutoId').append('<Option> Выберите модель машины</Option>');
        $.ajax({
            url: '/Detailing/Director/LoadModelCar?id=' + id,
            success: function (result) {
                $.each(result, function (i, data) {
                    $('#ModelAutoId').append('<Option value=' + data.value + '>' + data.text + '</Option>');
                });
            }
        });
    });
});
