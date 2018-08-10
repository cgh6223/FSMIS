var actionUrl = "/home/GetFile";
var editUrl = "/home/EditSysfun";
var addHtmlUrl = '/Scripts/Js/html/FindMore.html';
var edit_sysfunUrl = '/Scripts/Js/html/edit_sysfun.html';
$(function () {
  
    $('#a_down').linkbutton({ text: '下载文件' }).bind('click', curd.down);
    $('#a_find').linkbutton({ text: '高级搜索' }).bind('click', curd.find);    
    // alert($("#h_id").val());
    mygrid.databind();
    simpleSearch();
});

function simpleSearch() {
    var columns = $("#userTab").datagrid('options').columns[0];

    var str = "filename,keyword,username";
    $("#searchMenu").empty();
    $.each(columns, function (i, n) {
        if (str.indexOf(n.field) >= 0) {
            $('<div data-options="name:\'' + n.field + '\'">' + n.title + '</div>').appendTo("#searchMenu");
        }
    });
    $("#ss").searchbox({
        menu: '#searchMenu',
        searcher: function (value, name) {
            var arr = { "condition": name + " like '%" + value + "%'" };
            $("#userTab").datagrid('load', arr);
        }
    })
};



function mergeCell(tableid, mer) {
    var colArray = mer.split(',');
    var span = 1;
    var preTxt = "";
    var curTxt = "";
    var Rowcount = $("#" + tableid).datagrid('getRows').length;
    for (var j = 0; j < colArray.length; j++) {
        for (i = 0; i <= Rowcount; i++) {
            if (i == Rowcount) {
                curTxt = "";
            } else {
                curTxt = $("#" + tableid).datagrid('getRows')[i][colArray[j]];
            }

            if (preTxt == curTxt) {
                span += 1;//比对当前行和上一行是否一致，如果一致span+1
            } else {
                var index = i - span;
                $("#" + tableid).datagrid('mergeCells', {
                    index: index,
                    field: colArray[j],
                    rowspan: span,
                    colspan: null
                });
                span = 1;
                preTxt = curTxt;   //赋值
            }
        }
    }

}
function showS(name, urls) {
    parent.addTab(name, urls);
}

var mygrid = {
    databind: function () {
        $("#userTab").datagrid({
            url: actionUrl ,
            width: '98%',
            height: '98%',        
            rownumbers: true,
            striped: true,
            fit: true,
            fitColumns: true,
            toolbar: '#tb',
            columns: [[
                { field: 'keyid', title: ' ', width: 150,checkbox:true },
                { field: 'filename', title: '文件名称', width: 150 },
                { field: 'keyword', title: '关键字', width: 150 },
                { field: 'username', title: '创建人', width: 150 },
                { field: 'extname', title: '文件格式', width: 150 },
                {
                    field: 'viewdir', title: '预览', width: 150, formatter: function (value, row, index) {
                        if (row.viewmode == "3") {
                            return "";
                        } else {
                            var myurl = "/home/pdfobject?fileid=" + row.keyid;
                            return "<a href='javascript:showS(\"" + row.filename + "\",\"" + myurl + "\")'>预览</a>";
                        }
                    }
                },
                {
                    field: 'viewmode', title: '预览留痕', width: 150, formatter: function (value, row, index) {
                        if (row.viewmode == "3") {
                            return "";
                        } else {
                            return "<a href='javascript:curd.view(\"" + row.keyid + "\")' >查看</a>";
                        }
                    }
                }


            ]],
            pagination: true,
            pageSize: 20


        })
    },
    selectRow: function () {
        return $("#userTab").datagrid('getSelected');
    },
    reload: function () {
        $('#userTab').datagrid('clearSelections').datagrid('reload');
    }
}

var curd = {
    find: function () {      
       
            var p = $("<div>/").appendTo("body");
            var addDia = p.dialog({
                title: '高级搜索',
                width: 400,
                height: 420,
                modal: true,
                href: addHtmlUrl,
                onLoad: function () {
                    var mydate = new Date();
                    $('#cbegin').datebox('setValue', mydate.toLocaleDateString());
                    $('#cend').datebox('setValue', mydate.toLocaleDateString());
                },
                buttons: [{
                    text: '查询',
                    handler: function () {                     
                        var filename = $("#filename").val();
                        var keyword = $("#keyword").val();
                        var username = $("#username").val();
                        var cbegin = $("#cbegin").val();
                        var cend = $("#cend").val();
                        var con = "status=1 and createtime between '" + cbegin + "' and '" + cend + "'";
                        if (filename) {
                            con += " and filename like '%" + filename + "%'";
                        }
                        if (keyword) {
                            con += " and keyword like '%" + keyword + "%'";
                        }
                        if (username) {
                            con += " and username like '%" + username + "%'";
                        }
                        var arr = { 'condition': con };
                        $("#userTab").datagrid('load', arr);
                        addDia.dialog('destroy');
                    }
                }, {
                    text: '关闭',
                    handler: function () {
                        addDia.dialog('destroy');
                    }
                }]

            })

        
    },
    add: function () {

        var p = $("<div>/").appendTo("body");
        var addDia = p.dialog({
            title: '添加',
            width: 400,
            height: 420,
            modal: true,
            href: addHtmlUrl,
            onLoad: function () {
                $("#h_option").val("add");
            },
            buttons: [{
                text: '保存',
                handler: function () {
                    if ($("#FMCGForm").form("validate")) {
                        var file = document.getElementById("filebox_file_id_1").files[0];
                        if (file == null) {
                            parent.showTip("请选择要上传的文件");
                            return;
                        }
                        var forms = new FormData();

                        forms.append("file", file);
                        forms.append("filename", $("#filename").val());
                        forms.append("keyword", $("#keyword").val());
                        forms.append("Ispublic", $("#ispublic").attr("checked"));
                        forms.append("dirID", $("#h_id").val());
                        forms.append("option", "add");
                        var xhr = new XMLHttpRequest();

                        xhr.open("POST", "/Home/UploadFiles", true);

                        xhr.onload = function () {
                            alert("上传成功！");
                            mygrid.reload();
                            addDia.dialog("destroy");
                        };
                        xhr.upload.addEventListener("progress", function (evt) {
                            var values = Math.round(evt.loaded / evt.total * 100);
                            if (evt.lengthComputable) {
                                $('#progressFile').progressbar('setValue', 100);
                            } else {
                                $('#progressFile').progressbar('setValue', values);
                            }

                        }, false);
                        xhr.send(forms);

                    }
                }
            }, {
                text: '关闭',
                handler: function () {
                    addDia.dialog('destroy');
                }
            }]

        })

    },
    del: function () {
        var row = mygrid.selectRow();
        if (row) {
            $.getJSON("/home/DelFile?ID=" + row.keyid, function (data) {
                if (data.sucess) {
                    parent.showTip("删除成功");
                    mygrid.reload();
                }
            });
        } else {
            parent.showTip("请选择要删除的行！");
        }

    },
    down: function () {
        var row = mygrid.selectRow();
        if (row) {
            var selRows = $("#userTab").datagrid('getChecked');
            var arr=[];
            $.each(selRows, function (i, n) {
                arr.push(n.keyid);
            })
            var fileid= arr.join(',');
            location.href = "/home/DownFile?fileid=" + fileid;
        } else {
            parent.showTip("请选择要下载的行！");
        }

    },
    view: function (keyid) {
        var p = $("<div>/").appendTo("body");
        var addDia = p.dialog({
            title: '查看',
            width: 400,
            height: 420,
            modal: true,
            href: ViewUrl,
            onLoad: function () {
                $("#myuserTab").datagrid({
                    url: actionViewUrl + keyid,
                    width: '98%',
                    height: '98%',
                    rownumbers: true,
                    striped: true,
                    fit: true,
                    fitColumns: true,
                    columns: [[
                        { field: 'username', title: '访问用户', width: 150 },
                        { field: 'lasttime', title: '最后访问时间', width: 150 }
                    ]]
                })
            },
            buttons: [{
                text: '关闭',
                handler: function () {
                    addDia.dialog('destroy');
                }
            }]

        })

    }
}