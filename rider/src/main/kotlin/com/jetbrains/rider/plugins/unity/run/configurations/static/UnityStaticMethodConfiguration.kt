package com.jetbrains.rider.plugins.unity.run.configurations.static

import com.intellij.execution.Executor
import com.intellij.execution.configurations.ConfigurationFactory
import com.intellij.execution.configurations.LocatableConfiguration
import com.intellij.execution.configurations.RunConfiguration
import com.intellij.execution.configurations.RunProfileState
import com.intellij.execution.runners.ExecutionEnvironment
import com.intellij.openapi.diagnostic.Logger
import com.intellij.openapi.options.SettingsEditor
import com.intellij.openapi.project.Project
import javax.swing.Icon

class UnityStaticMethodConfiguration(
    private val name: String,
    private val project: Project,
    private val factory: UnityStaticMethodConfigurationFactory,
    val parameters: UnityStaticMethodConfigurationParameters)
    : RunConfiguration, LocatableConfiguration {

    companion object {
        private val logger = Logger.getInstance(UnityStaticMethodConfiguration::class.java)
    }

    override fun hideDisabledExecutorButtons(): Boolean {
        return true;
    }

    override fun getState(executor: Executor, environment: ExecutionEnvironment): RunProfileState? {
        TODO("Not yet implemented")
    }

    override fun getName(): String {
        return name
    }

    override fun getIcon(): Icon? {
        return factory.icon
    }

    override fun clone(): RunConfiguration {
        TODO("Not yet implemented")
    }

    override fun getFactory(): ConfigurationFactory? {
        return factory
    }

    override fun setName(name: String?) {
        TODO("Not yet implemented")
    }

    override fun getConfigurationEditor(): SettingsEditor<out UnityStaticMethodConfiguration> {
        return UnityStaticMethodConfigurationEditor(project)
    }

    override fun getProject(): Project {
        return project
    }

    override fun isGeneratedName(): Boolean {
        return true
    }
}

