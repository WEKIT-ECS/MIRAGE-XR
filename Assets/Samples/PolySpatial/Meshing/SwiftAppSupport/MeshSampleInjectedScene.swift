// Any swift file whose name ends in "InjectedScene" is expected to contain
// a computed static "scene" property like the one below. It will be injected to the top
// level App's Scene. The name of the class/struct must match the name of the file.

import Foundation
import SwiftUI

struct MeshSampleInjectedScene {
    @SceneBuilder
    static var scene: some Scene {
        WindowGroup(id: "MeshSample") {
            // The sample defines a custom view, but you can also put your entire window's
            // structure here as you can with SwiftUI.
            MeshDemoContentView()
        }.defaultSize(width: 450.0, height: 420.0)

        // You can create multiple WindowGroups here for different wnidows;
        // they need a distinct id. If you include multiple items,
        // the scene property must be decorated with "@SceneBuilder" as above.
        WindowGroup(id: "SimpleWindow") {
            Text("Mesh Demo")
        }
    }
}
