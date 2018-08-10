/*
    desc:jQuery模拟签章插件
    author:hyf
    date:2012-12-24
    blog:http://blog.csdn.net/sweetsuzyhyf
*/
$.fn.zSign = function (options) {
    var _s = $.extend({
        img: '',                        //图片地址
        width: 150,                     //签章图片大小
        height: 150,
        btnPanel: true,                 //是否开启按钮面板，若按钮面板不满足需求可以关闭后通过返回的handle对象直接调用方法
        callBack: null,                 //保存按钮回调函数
        list: null,                     //初始化签章，参数格式参照callBack回调函数返回的数据格式
        isPercentage: false              //返回结果中的left、top是否以百分比显示，若夫容器是自适应布局推荐
    }, options || {});
 
    var _parent = $(this).addClass('zsign'), _pw = _parent.width(), _ph = _parent.height();
    var range = {
        minX: 0,
        minY: 0,
        maxX: _pw - _s.width - 2,      //扣去2个边框1px
        maxY: _ph - _s.height - 2
    };
 
    //按钮面板
    var _btnPanel = $("<div class='panel'><button class='btn add' >盖 章</button><button class='btn save'>保 存</button><button class='btn print'>打 印</button></div>").appendTo(_parent);
    _btnPanel.css('display', _s.btnPanel ? 'block' : 'none');
 
    //添加
    var _add = $('.add', _btnPanel).click(function (e) {
        handle.add();
    });
 
    //保存 
    $('.save', _btnPanel).click(function () {
        handle.save();
    });
 //打印
    $('.print', _btnPanel).click(function () {
        handle.print();
    });
    if (_s.list) {
        handle.init(_s.list);
    }
 
    var handle = {
        list: [],
 
        //初始化签章
        init: function (list) {
            handle.list = [];
            $('.sign', _parent).remove();
            for (var i = 0; i < list.length; i++) {
                var item = list[i];
                _parent.append("<div class='sign ok' style='height:" + _s.height + "px;width:" + _s.width + "px;background-image:" + item.img + ";top:" + item.top + ";left:" + item.left + "'></div>");
            }
        },
 
        //添加签章
        add: function () {
            handle.btnAddToggle();
            var sign = $("<div class='sign' style='height:" + _s.height + "px;width:" + _s.width + "px;background-image:url(" + _s.img + ");'><button class='btn ok' >确定</button><button class='btn del' >删除</button></div>").appendTo(_parent);
            $('.ok', sign).click(function () {
                //确定签章
                handle.sign(sign);
            });
            $('.del', sign).click(function () {
                //删除签章
                handle.del(sign);
            });
            handle.move(sign);
        },
 
        //确定签章
        sign: function (obj) {
            obj.addClass('ok').off('mousedown').find('.btn').css('display', 'none');
            handle.btnAddToggle();
            handle.list.push({ img: obj.css('background-image') + "", top: obj.css('top'), left: obj.css('left') });
        },
 
        //删除
        del: function (obj) {
            obj.remove();
            handle.btnAddToggle();
        },
 //打印
        print: function () {
            $("#viewer").print({
                //Use Global styles
                globalStyles: false,
                //Add link with attrbute media=print
                mediaPrint: false,
                //Custom stylesheet
                stylesheet: "http://fonts.googleapis.com/css?family=Inconsolata",
                //Print in a hidden iframe
                iframe: false,
                //Don't print this
                noPrintSelector: ".avoid-this",
                //Add this at top
                ////prepend: "Hello World!!!<br/>",
                //Add this on bottom
                //// append: "<br/>Buh Bye!",
                //Log to console when printing is done via a deffered callback
                deferred: $.Deferred().done(function () { console.log('Printing done', arguments); }),
                title: '版本'
            });
        },
        //移动
        move: function (obj) {
            //绑定移动事件
            obj.on('mousedown', function (e) {
                obj.data('x', e.clientX);
                obj.data('y', e.clientY);
                var position = obj.position();
                $(document).on('mousemove', function (e1) {
                    var x = e1.clientX - obj.data('x') + position.left;
                    var y = e1.clientY - obj.data('y') + position.top;
                    x = x < range.minX ? range.minX : x;
                    x = x > range.maxX ? range.maxX : x;
                    y = y < range.minY ? range.minY : y;
                    y = y > range.maxY ? range.maxY : y;
 
                    obj.css({ left: x, top: y });
                }).on('mouseup', function () {
                    $(this).off('mousemove').off('mouseup');
                });
            });
        },
 
        //保存
        save: function () {
            var r = true;
            if ($('.sign:not(.ok)', _parent).length != 0) {
                if (!confirm("未点击确认的签章将被移除，确定保存吗？")) {
                    r = false;
                }
            }
            if (r) {
                //删除未确定位置的签章
                $('.sign:not(.ok)', _parent).remove();
                _btnPanel.remove();
                if (_s.callBack) {
                    if (_s.isPercentage) {
                        for (var i = 0; i < handle.list.length; i++) {
                            var item = handle.list[i];
                            item.top = parseInt(item.top) / _ph * 100 + '%';
                            item.left = parseInt(item.left) / _pw * 100 + '%';
                        }
                    } else {
                        tmp = handle.list;
                    }
                    _s.callBack.call(this, handle.list);
                }
            }
        },
 
        //盖章按钮的状态切换
        btnAddToggle: function () {
            var disabled = _add.attr('disabled');
            if (disabled) {
                //判断是否有未确定的签章，若有则不切换
                if ($('.sign:not(.ok)', _parent).length == 0) {
                    _add.removeAttr('disabled');
                }
            } else {
                _add.attr('disabled', 'disabled');
            }
        },
 
        //返回参数列表，可以在外部直接修改
        s: _s,
 
        //签章移动范围
        range: range
    }
    return handle;
}
