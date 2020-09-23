package com.jetbrains.rider.plugins.unity.run.configurations.static

import com.intellij.openapi.project.Project
import com.jetbrains.rd.util.lifetime.Lifetime
import com.jetbrains.rider.run.configurations.LifetimedSettingsEditor
import com.jetbrains.rider.run.configurations.controls.ControlViewBuilder
import com.jetbrains.rider.run.configurations.controls.ProjectSelector
import com.jetbrains.rider.run.configurations.controls.StaticMethodEditor
import com.jetbrains.rider.run.configurations.runnableProjectsModelIfAvailable
import javax.swing.JComponent

class UnityStaticMethodConfigurationEditor(private val project: Project) :
    LifetimedSettingsEditor<UnityStaticMethodConfiguration>() {

    lateinit var viewModel: UnityStaticMethodConfigurationViewModel

    override fun resetEditorFrom(configuration: UnityStaticMethodConfiguration) {
        configuration.parameters.apply {
            viewModel.reset(
                projectFilePath,
                className,
                methodName
            )
        }
    }

    @Suppress("Duplicates")
    override fun applyEditorTo(configuration: UnityStaticMethodConfiguration) {
        val selectedProject = viewModel.projectSelector.project.valueOrNull
        if (selectedProject != null) {
            configuration.parameters.projectFilePath = selectedProject.projectFilePath
            configuration.parameters.apply {
                val lastDotIndex = viewModel.staticMethodEditor.text.value.lastIndexOf(".")
                if (lastDotIndex > 0) {
                    className = viewModel.staticMethodEditor.text.value.substring(0, lastDotIndex)
                    methodName = viewModel.staticMethodEditor.text.value.substring(lastDotIndex + 1)
                } else {
                    className = ""
                    methodName = ""
                }
            }
        }
    }

    override fun createEditor(lifetime: Lifetime): JComponent {
        viewModel = UnityStaticMethodConfigurationViewModel(
            lifetime,
            project.runnableProjectsModelIfAvailable,
            project,
            ProjectSelector("Project:"),
            StaticMethodEditor("Static method:", lifetime)
        )
        return ControlViewBuilder(lifetime, project).build(viewModel)
    }
}