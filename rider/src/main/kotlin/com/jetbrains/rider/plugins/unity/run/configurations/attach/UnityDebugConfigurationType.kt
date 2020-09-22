package com.jetbrains.rider.plugins.unity.run.configurations.attach

import com.intellij.execution.configurations.ConfigurationTypeBase
import com.intellij.openapi.project.DumbAware
import icons.UnityIcons

class UnityDebugConfigurationType : ConfigurationTypeBase(id,
    "Attach to Unity Editor", "Attach to Unity process and debug",
    UnityIcons.RunConfigurations.AttachToUnityParentConfiguration), DumbAware {

    val attachToEditorFactory = UnityAttachToEditorFactory(this)
    val attachToEditorAndPlayFactory = UnityAttachToEditorAndPlayFactory(this)

    init {
        addFactory(attachToEditorFactory)
        addFactory(attachToEditorAndPlayFactory)
    }

    companion object {
        // Note that this value is incorrect. The JavaDoc states that the ID should be camel cased without dashes or
        // underscores, etc. But it's used as the key for persisting run configuration settings, and so shouldn't ever
        // be changed. Too late now.
        const val id = "UNITY_DEBUG_RUN_CONFIGURATION"
    }
}
