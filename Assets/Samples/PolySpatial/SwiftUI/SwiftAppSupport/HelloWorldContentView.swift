//
// This custom View is referenced by SwiftUISampleInjectedScene
// to provide the body of a WindowGroup. It's part of the Unity-VisionOS
// target because it lives inside a "SwiftAppSupport" directory (and Unity
// will move it to that target).
//

import Foundation
import SwiftUI
import UnityFramework

struct HelloWorldContentView: View {
    
    @State var counterObject = ObjectCounter()
    
    var body: some View {
        VStack {
            Text("Hello, SwiftUI!")
            Divider()
                .padding(10)
            Button("Spawn Red Object") {
                CallCSharpCallback("spawn red")
                UpdateValues(counter: counterObject)
            }
            Button("Spawn Green Object") {
                CallCSharpCallback("spawn green")
                UpdateValues(counter: counterObject)
            }
            Button("Spawn Blue Object") {
                CallCSharpCallback("spawn blue")
                UpdateValues(counter: counterObject)
            }
        }
        .onAppear {
            // Call the public function that was defined in SwiftUISamplePlugin
            // inside UnityFramework
            CallCSharpCallback("appeared")
            UpdateValues(counter: counterObject)
        }
        
        HStack {
            Text("Cube Count: \(counterObject.cubeCount)")
                .padding(5)
            Text("Sphere Count: \(counterObject.sphereCount)")
                .padding(5)
        }
        .padding(10)
    }
}

func UpdateValues(counter: ObjectCounter) {
    counter.cubeCount = GetCubeCount()
    counter.sphereCount = GetSphereCount()
}


#Preview(windowStyle: .automatic) {
    HelloWorldContentView()
}

