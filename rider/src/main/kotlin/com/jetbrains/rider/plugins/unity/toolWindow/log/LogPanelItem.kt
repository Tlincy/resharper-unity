package com.jetbrains.rider.plugins.unity.toolWindow.log

import com.jetbrains.rider.model.unity.LogEventMode
import com.jetbrains.rider.model.unity.LogEventType

data class LogPanelItem(val time : Long,
                        val type : LogEventType,
                        val mode : LogEventMode,
                        val message : String,
                        val stackTrace : String,
                        val count: Int)