CargarTablaConfiguraciones();

function CargarTablaConfiguraciones() {
    
    $.ajax({
        url: baseUrl + 'ctConfiguracion/ListadoDeConfiguraciones',
        method: 'POST',
        data: {  },
        beforeSend: function (xhr) {
            xhr.setRequestHeader("XSRF-TOKEN", $('#RequestVerificationToken').val());
        },
        success: function (data) {
            $('#gridConfiguraciones').html(data);

            var TableDatatablesButtons = function () {
                var e = function () {
                    var e = $("#tablaConfiguraciones");
                    e.dataTable({
                        language: {
                            url: baseUrl + "global/language/Spanish.json",
                            aria: {
                                sortAscending: ": activate to sort column ascending",
                                sortDescending: ": activate to sort column descending"
                            },
                            emptyTable: "No data available in table",
                            info: "Showing _START_ to _END_ of _TOTAL_ entries",
                            infoEmpty: "No entries found",
                            infoFiltered: "(filtered1 from _MAX_ total entries)",
                            lengthMenu: "_MENU_ entries",
                            search: "Search:",
                            zeroRecords: "No matching records found"
                        },
                        buttons: [],
                        order: [],
                        lengthMenu: [[10, 15, 20, -1], [10, 15, 20, "All"]],
                        pageLength: 20,
                        pagingType: "bootstrap_full_number",
                        dom: "<'row' <'col-md-12'B>><'row'<'col-md-6 col-sm-12'l><'col-md-6 col-sm-12'f>r><'table-scrollable't><'row'<'col-md-5 col-sm-12'i><'col-md-7 col-sm-12'p>>"
                    });

                    $('#tablaConfiguraciones').on('click', 'tr', function () {
                        if ($(this).hasClass('selected')) {
                            $(this).removeClass('selected');
                            idFilaSeleccionada = null;
                            document.getElementById('editar').setAttribute('disabled', true);
                            document.getElementById('eliminar').setAttribute('disabled', true);
                        }
                        else {
                            e.$('tr.selected').removeClass('selected');
                            $(this).addClass('selected');
                            idFilaSeleccionada = $(this).data('info');
                            document.getElementById('editar').removeAttribute('disabled');
                            document.getElementById('eliminar').removeAttribute('disabled');
                        }
                    });
                }

                return {
                    init: function () {
                        jQuery().dataTable && (e())
                    }
                }
            }();

            TableDatatablesButtons.init();
        },
        error: function (error) {
            console.log(error);
        },
        complete: function () {

        }
    });
}