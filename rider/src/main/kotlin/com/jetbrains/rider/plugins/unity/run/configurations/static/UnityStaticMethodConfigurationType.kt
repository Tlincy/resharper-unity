package com.jetbrains.rider.plugins.unity.run.configurations.static


import com.intellij.execution.configurations.ConfigurationTypeBase
import com.intellij.openapi.project.PossiblyDumbAware
import com.intellij.openapi.project.Project
import com.jetbrains.rider.isUnityGeneratedProject
import icons.RiderIcons

class UnityStaticMethodConfigurationType : ConfigurationTypeBase(
    "UnityStaticMethod",
    "Unity Static Method",
    "Unity Static Method",
    RiderIcons.RunConfigurations.StaticMethod
), PossiblyDumbAware {

    val factory: UnityStaticMethodConfigurationFactory = UnityStaticMethodConfigurationFactory(this)

    fun isApplicable(project:Project) = project.isUnityGeneratedProject()

    init {
        addFactory(factory)
    }
}
