var actionUrl = "/home/GetFileByDir?dirid=";
var editUrl = "/home/EditSysfun";
var addHtmlUrl = '/Scripts/Js/html/sysfun_add.html';
var ViewUrl = '/Scripts/Js/html/Viewinfo.html';
var actionViewUrl = "/home/GetView?fid=";
$(function () {
    $('#a_edit').linkbutton({ text: '修改文件' }).bind('click', curd.update);
    $('#a_add').linkbutton({ text: '添加文件' }).bind('click', curd.add);
    $('#a_del').linkbutton({ text: '删除文件' }).bind('click', curd.del);
    $('#a_down').linkbutton({ text: '下载文件' }).bind('click', curd.down);  
   // alert($("#h_id").val());
    mygrid.databind();
    simpleSearch();
});

function simpleSearch() {
    var columns = $("#userTab").datagrid('options').columns[0];
   
    var str = "filename";
    $("#searchMenu").empty();
    $.each(columns, function (i, n) {
        if (str.indexOf(n.field) >= 0) {           
            $('<div data-options="name:\'' + n.field + '\'">' + n.title + '</div>').appendTo("#searchMenu");
        }
    });
    $("#ss").searchbox({
        menu: '#searchMenu',
        searcher: function (value, name) {
            var arr = { "condition": name + " like '%" + value+"%'" };
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

function showS(name,urls) {
    parent.addTab(name, urls);
}

var mygrid = {    
    databind: function () {
        $("#userTab").datagrid({
            url: actionUrl + $("#h_id").val(),
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
                { field: 'extname', title: '文件格式', width: 150 },
                {
                    field: 'viewdir', title: '预览', width: 150, formatter: function (value, row, index) {
                        if (row.viewmode == "3") {
                            return "";
                        } else if (row.viewmode == "1") {
                            var myurl = "/home/pdfobject?fileid=" + row.keyid;
                            return "<a href='javascript:showS(\"" + row.filename + "\",\"" + myurl + "\")'>预览</a>";
                        } else {
                            return "<a href='/" + row.viewdir + "' target='_blank'>预览</a>";
                        }
                    }
                },
                {
                    field: 'viewmode', title: '预览留痕', width: 150, formatter: function (value, row, index) {
                        if (row.viewmode == "3") {
                            return "";
                        } else {
                            return "<a href='javascript:curd.view(\""+row.keyid+"\")' >查看</a>";
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
function showmask(ids) {
    //alert("www");
    var left = ($(window).outerWidth(true) - 190) / 2;
    var top = ($(window).height() - 35) / 2;
    var height = $(window).height() * 2;
   // alert(height);
    $("<div class=\"datagrid-mask\"></div>").css({ display: "block", width: "100%", height: "100%" }).appendTo(ids);
    $("<div class=\"datagrid-mask-msg\"></div>").html("正在加载,请稍候...").css({ display: "block", left: 120, top: 200 }).appendTo(ids);
}

var curd = {    
    update: function () {
        var row = mygrid.selectRow();
        if (row) {
            var p = $("<div>/").appendTo("body");
            var addDia = p.dialog({
                title: '修改',
                width: 400,
                height: 420,
                modal: true,
                href: addHtmlUrl,
                onLoad: function () {
                    $("#h_option").val("update");
                    $("#file1").hide();
                    $("#filename").val(row.filename);
                    $("#keyword").val(row.keyword);
                },
                buttons: [{
                    text: '保存',
                    handler: function () {
                        if ($("#FMCGForm").form("validate")) {
                            $.ajax({
                                type: "POST",
                                dataType: "json",
                                url: "/Home/UploadFiles",
                                data: { filename: $("#filename").val(), keyword: $("#keyword").val(), ispublic: $("#ispublic").attr("checked"), id: row.keyid,option:"update" },
                                success: function (data) {
                                    if (data.sucess) {
                                        parent.showTip("更新成功！");
                                        mygrid.reload();
                                        addDia.dialog('destroy');
                                    } else {
                                        parent.showTip("更新失败！");
                                    }
                                }
                            });
                        }
                    }
                }, {
                    text: '关闭',
                    handler: function () {
                        addDia.dialog('destroy');
                    }
                }]

            })
           
        } 
    },
    add: function () {
       
            var p = $("<div>/").appendTo("body");
            var addDia = p.dialog({
                title: '添加',
                width: 400,
                height: 420,
                modal:true,
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
                            if (file.size == 0) {
                                parent.showTip("不能上传空文档");
                                return;
                            }
                            showmask("#FMCGForm");
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
            $.getJSON("/home/DelFile?ID="+row.keyid, function (data) {
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
            var arr = [];
            $.each(selRows, function (i, n) {
                arr.push(n.keyid);
            });
            var msg = arr.join(",");
            // alert(msg);
            location.href = "/home/DownFile?fileid=" + msg;
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
            onOpen: function () {                
                var myurl = actionViewUrl + keyid;
                $.getJSON(myurl, function (data) {
                    if (data.rows.length > 0) {
                       
                        var html = "<tr style='background-color:#ffd800;text-align:center'><td>访问人</td><td>访问时间</td><tr>";
                        $.each(data.rows, function (i, n) {
                            html += "<tr style='text-align:center'><td>" + n.username + "</td><td>" + n.lasttime + "</td><tr>";
                        })
                        $("#mytab").append(html);
                    }
                })
            },
            onClose: function () {
                addDia.dialog('destroy');
            },
            buttons: [{
                text: '关闭',
                handler: function () {
                    addDia.dialog('destroy');
                }
            }]

        })

    },
    show: function (name, url, mode) {

    }
   

}