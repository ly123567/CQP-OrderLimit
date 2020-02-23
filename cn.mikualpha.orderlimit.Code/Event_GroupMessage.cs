using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Native.Sdk.Cqp.EventArgs;
using Native.Sdk.Cqp.Interface;

public class Event_GroupMessage : IGroupMessage
{
    #region --公开方法--
    /// <summary>
    /// Type=2 群消息<para/>
    /// 处理收到的群消息
    /// </summary>
    /// <param name="sender">事件的触发对象</param>
    /// <param name="e">事件的附加参数</param>
    public void GroupMessage(object sender, CQGroupMessageEventArgs e)
    {
        ApiModel.setModel(e.CQApi, e.CQLog);
        e.Handler = MessageCount.GetInstance().processGroupMessage(e);
    }
    #endregion
}

    