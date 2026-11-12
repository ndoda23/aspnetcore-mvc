var dataTable;

$(document).ready(function () {
    loadDataTable();

    $('#btnApply').on('click', function () {
        dataTable.ajax.url(buildUrl()).load();
    });

    $('#btnReset').on('click', function () {
        $('#filterEntity').val('all');
        $('#filterFrom').val('');
        $('#filterTo').val('');
        dataTable.ajax.url(buildUrl()).load();
    });
});

function buildUrl() {
    var params = new URLSearchParams();
    params.append('entity', $('#filterEntity').val());

    var from = $('#filterFrom').val();
    var to = $('#filterTo').val();
    if (from) params.append('from', from);
    if (to) params.append('to', to);

    return '/admin/auditlog/getall?' + params.toString();
}

function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        ajax: buildUrl(),
        order: [[0, 'desc']],
        columns: [
            {
                data: 'timestamp', width: '15%',
                render: function (data, type) {
                    if (type !== 'display') return data;      
                    return formatDate(data);
                }
            },
            { data: 'userEmail', width: '18%', render: escapeHtml },
            {
                data: 'action', width: '12%',
                render: function (data) {
                    return '<span class="badge ' + actionBadge(data) + '">' + escapeHtml(data) + '</span>';
                }
            },
            { data: 'entityName', width: '10%', render: escapeHtml },
            {
                data: 'entityId', width: '15%',
                render: function (data) {
                    if (!data) return '';
                    var text = data.length > 12 ? data.substring(0, 8) + '…' : data;   
                    return '<span title="' + escapeHtml(data) + '">' + escapeHtml(text) + '</span>';
                }
            },
            {
                data: null, width: '30%', orderable: false,
                render: function (data, type, row) {
                    var oldV = row.oldValue ? '<span class="text-danger">' + escapeHtml(row.oldValue) + '</span>' : '';
                    var newV = row.newValue ? '<span class="text-success">' + escapeHtml(row.newValue) + '</span>' : '';
                    if (oldV && newV) return oldV + ' → ' + newV;
                    return oldV || newV;
                }
            }
        ]
    });
}

function formatDate(value) {
    if (!value) return '';
    if (!value.endsWith('Z') && !/[+-]\d\d:\d\d$/.test(value)) value += 'Z';
    return new Date(value).toLocaleString('en-GB', {
        day: '2-digit', month: '2-digit', year: 'numeric',
        hour: '2-digit', minute: '2-digit', second: '2-digit'
    });
}

function actionBadge(action) {
    switch (action) {
        case 'Created': return 'bg-success';
        case 'Updated': return 'bg-primary';
        case 'Deleted': return 'bg-danger';
        case 'StatusChanged': return 'bg-info text-dark';
        case 'UserLocked': return 'bg-danger';
        case 'UserUnlocked': return 'bg-success';
        case 'RoleChanged': return 'bg-warning text-dark';
        case 'PasswordReset': return 'bg-secondary';
        default: return 'bg-secondary';
    }
}

function escapeHtml(value) {
    if (value === null || value === undefined) return '';
    return String(value)
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;')
        .replace(/'/g, '&#39;');
}