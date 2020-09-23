package com.jetbrains.rider.plugins.unity.run.configurations.static

import com.intellij.execution.configurations.ConfigurationFactory
import com.intellij.execution.configurations.ConfigurationType
import com.intellij.execution.configurations.RunConfiguration
import com.intellij.openapi.project.Project

class UnityStaticMethodConfigurationFactory(type: ConfigurationType): ConfigurationFactory(type) {

    override fun isConfigurationSingletonByDefault(): Boolean {
        return true
    }

    private fun createParameters(project: Project) = UnityStaticMethodConfigurationParameters(
        project,
        "",
        "",
        ""
    )

    override fun createConfiguration(name: String?, template: RunConfiguration): RunConfiguration =
        UnityStaticMethodConfiguration(name ?: "Unity Static Method", template.project, this, createParameters(template.project))

    override fun createTemplateConfiguration(project: Project): RunConfiguration =
        UnityStaticMethodConfiguration(name, project, this, createParameters(project))
}