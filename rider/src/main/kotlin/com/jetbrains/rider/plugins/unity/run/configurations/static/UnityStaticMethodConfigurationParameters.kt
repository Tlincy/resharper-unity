package com.jetbrains.rider.plugins.unity.run.configurations.static

import com.intellij.openapi.project.Project

class UnityStaticMethodConfigurationParameters(
    val project:Project,
    var projectFilePath: String,
    var className: String,
    var methodName: String
)
