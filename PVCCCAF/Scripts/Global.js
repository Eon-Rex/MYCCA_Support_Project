function InitialiseJqueryDataTable(tblid) {
    var buttonCommon = {
        exportOptions: {
            format: {
                body: function (data, row, column, node) {
                    return column === 5 ?
                        data.replace(/[$,]/g, '') :
                        data;
                }
            }
        }
    };
    //debugger;
    $(tblid).DataTable({
        "processing": true, dom: 'Blpfrtip', buttons: [$.extend(true, {}, buttonCommon, {
            extend: 'copyHtml5', text: '<u>C</u>opy',
            key: { key: 'c', altKey: true },
            responsive: true
            //className: 'btn btn-block common_btn'
        })
    , $.extend(true, {}, buttonCommon, {
        extend: 'excelHtml5', text: 'E<u>x</u>port to Excel',
        key: { key: 'x', altKey: true }//, 
        //className: 'btn btn-block common_btn'
    })]
    })
};

function InitialiseJqueryDataTable1(tblid) {
    var buttonCommon = {
        exportOptions: {
            format: {
                body: function (data, row, column, node) {
                    return column === 5 ?
                        data.replace(/[$,]/g, '') :
                        data;
                }
            }
        }
    };
    //debugger;
    $(tblid).DataTable({
        "processing": true,
        dom: 'Blpfrtip'
        
    })
};