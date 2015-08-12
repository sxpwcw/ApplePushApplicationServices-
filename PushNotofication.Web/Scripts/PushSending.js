var Push = {};
Push.GetAllTokens = function (control) {
    $(control).append("<option>读取设备ID中......</option>")
    var tmp = "<option value=\"{Token}\">{Token}</option>";
    $.post("/api/AllTokens", function (result) {
        if (result.length > 0) {
            $(control).children().remove();
            $(control).append("<option value=\"0\">【请选择】</option>")
            $(control).append("<option value=\"All\">全部设备</option>")
            $.each(result, function (i, item) {
                $(control).append(tmp.replace(/{Token}/ig, item.Token));
            });
        }
    }, "json");
}
Push.Send = function () {
    var stype = $("#pType");
    var aduio = $("#audio");
    var Token = $("#DeviceToken");
    var msg = $("#Msg");
    var badge = $("#Badge");
    if (stype.val() == "0") {
        alert("请选择发送类别");
        stype.focus();
        return;
    }else if (aduio.val() == "0") {
        alert("请选择推送至客户端播放的声音！");
        aduio.focus();
        return;
    } else if (Token.val() == "0") {
        alert("请选择要发送的客户端ID！");
        Token.focus();
        return;
    } else if (msg.val() == "") {
        alert("请输入要发送的内容！");
        msg.focus();
        return;
    } else if (isNaN(badge.val())) {
        alert("标识数量只能输入数字！");
        badge.focus();
        return;
    } else if (badge.val() == "") {
        alert("请输入标识数量！");
        badge.focus();
        return;
    } else {
        $("#submit").val("发送中...").attr({ "disabled": "disabled" });

        $.post("/api/PostNotificationWithOptions", { DeviceToken: Token.val(), pType: stype.val(), Msg: msg.val(), Badge: badge.val(), AudioName: aduio.val() }, function () {
            alert("推送请求已经提交，点击确定跳转至历史信息查看页面，稍后推送成功后您可以查看到信息是否推送成功！");
            window.location.href = "/Home/History";
        });
    }
}