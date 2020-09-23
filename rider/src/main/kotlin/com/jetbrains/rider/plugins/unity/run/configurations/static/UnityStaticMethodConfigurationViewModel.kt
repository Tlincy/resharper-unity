package com.jetbrains.rider.plugins.unity.run.configurations.static

import com.intellij.openapi.project.Project
import com.jetbrains.rd.util.lifetime.Lifetime
import com.jetbrains.rd.util.reactive.adviseOnce
import com.jetbrains.rider.model.*
import com.jetbrains.rider.projectView.ProjectModelViewHost
import com.jetbrains.rider.run.configurations.controls.*
import com.jetbrains.rider.projectView.solution
import java.io.File

class UnityStaticMethodConfigurationViewModel(
    private val lifetime: Lifetime,
    private val project: Project,
    val projectSelector: ProjectSelector,
    val staticMethodEditor: StaticMethodEditor
) : RunConfigurationViewModelBase() {

    override val controls: List<ControlBase> =
        listOf(
            projectSelector,
            staticMethodEditor)
    private val type = UnityStaticMethodConfigurationType()

    init {
        disable()
//        project.solution.
//            projectSelector.bindTo(runnableProjectsModel, lifetime, { p -> type.isApplicable(project) }, ::enable, ::handleProjectSelection)
    }

    private fun handleProjectSelection(runnableProject: RunnableProject) {
        if (!isLoaded) return
        ProjectModelViewHost.tryGetInstance(project)?.let { projectModelViewHost ->
            staticMethodEditor.refreshCompletionList.fire(projectModelViewHost.getProjectModeId(runnableProject.projectFilePath))
        }
    }

    @Suppress("Duplicates")
    fun reset(projectFilePath: String,
              className: String,
              methodName: String
    ) {
        isLoaded = false

        ProjectModelViewHost.tryGetInstance(project).model.view.

        runnableProjectsModel?.projects?.adviseOnce(lifetime) { projectList ->
            if (projectFilePath.isEmpty() || projectList.none {
                    it.projectFilePath == projectFilePath && type.isApplicable(it.kind)
                }) {
                // Case when project didn't selected otherwise we should generate fake project to avoid drop user settings.
                if (projectFilePath.isEmpty()) {
                    projectList.firstOrNull { type.isApplicable(it.kind) }?.let { project ->
                        projectSelector.project.set(project)
                        isLoaded = true
                        handleProjectSelection(project)
                    }
                } else {
                    val fakeProjectName = File(projectFilePath).name
                    val fakeProject = RunnableProject(
                        fakeProjectName, fakeProjectName, projectFilePath, RunnableProjectKind.Unloaded,
                        listOf(ProjectOutput(projectTfm, "", listOf(), workingDirectory, "")),
                        envs.map { EnvironmentVariable(it.key, it.value) }.toList(), null, listOf()
                    )
                    projectSelector.projectList.apply {
                        clear()
                        addAll(projectList + fakeProject)
                    }
                    projectSelector.project.set(fakeProject)
                }
            } else {
                projectList.singleOrNull {
                    it.projectFilePath == projectFilePath && type.isApplicable(it.kind)
                }?.let { project ->
                    projectSelector.project.set(project)
                    val projectOutput = project.projectOutputs.single ()
                }
            }

            if(className.isBlank() && methodName.isBlank()) {
                staticMethodEditor.text.set("")
                staticMethodEditor.defaultValue.set("")
            } else {
                staticMethodEditor.text.set("$className.$methodName")
                staticMethodEditor.defaultValue.set("$className.$methodName")
            }
            projectSelector.project.valueOrNull?.let {
                staticMethodEditor.refreshCompletionList.fire(ProjectModelViewHost.getInstance(project).getProjectModeId(projectFilePath))
            }
            isLoaded = true
        }
    }
}