var actionUrl = "/home/GetShowList?deptid=";





var mygrid = {
    databind: function () {
        
    },
    selectRow: function () {
        return $("#userTab").datagrid('getSelected');
    },
    reload: function () {
        $('#userTab').datagrid('clearSelections').datagrid('reload');
    }
}

