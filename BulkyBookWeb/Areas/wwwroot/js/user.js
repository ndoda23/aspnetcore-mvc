var dataTable;

$(document).ready(function () {
    loadDataTable();
});

function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        ajax: '/admin/user/getall',
        columns: [
            { data: 'name', width: '20%' },
            { data: 'email', width: '20%' },
            { data: 'phoneNumber', width: '15%' },
            {
                data: 'role', width: '10%',
                render: function (data) {
                    return '<span class="badge bg-secondary">' + (data ?? 'No role') + '</span>';
                }
            },
            {
                data: 'id', width: '35%',
                render: function (data, type, row) {
                    var lockBtn = row.isLocked
                        ? `<button onclick="lockUnlock('${data}')" class="btn btn-sm btn-danger">
                               <i class="bi bi-lock-fill"></i> Lock
                           </button>`
                        : `<button onclick="lockUnlock('${data}')" class="btn btn-sm btn-success">
                               <i class="bi bi-unlock-fill"></i> Unlock
                           </button>`;

                    return `<div class="d-flex gap-2 justify-content-end">
                                ${lockBtn}
                                <a href="/admin/user/rolemanagement?userId=${data}" class="btn btn-sm btn-secondary">
                                    <i class="bi bi-person-badge"></i> Role
                                </a>
                                <a href="/admin/user/userpasswordchange?userId=${data}" class="btn btn-sm btn-warning">
                                    <i class="bi bi-key-fill"></i> Password
                                </a>
                            </div>`;
                }
            }
        ]
    });
}

function lockUnlock(userId) {
    fetch('/admin/user/lockunlock', {
        method: 'POST',
        headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
        body: 'userId=' + encodeURIComponent(userId)
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                dataTable.ajax.reload(null, false);   
                toastr.success(data.message);          
            } else {
                toastr.error(data.message);
            }
        })
        .catch(err => console.error(err));              
}