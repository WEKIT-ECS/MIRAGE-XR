//
// This custom View is referenced by SwiftUISampleInjectedScene
// to provide the body of a WindowGroup. It's part of the Unity-VisionOS
// target because it lives inside a "SwiftAppSupport" directory (and Unity
// will move it to that target).
//

import Foundation
import SwiftUI
import UnityFramework

struct MeshDemoContentView: View {
    
    @State var isShowingMesh = true;
    @State var isSpawningObjects = false;
    @State var occlusionMaterialActive = true;
    @State var wireframeMaterialActive = false;
    @State var textureMaterialActive = false;
    @State var currentMeshViz: MeshVisualization = .texture

    
    var body: some View {
        VStack {
            Text("Mesh Demo")
                .font(.title)
            Toggle("Mesh Enabled", isOn: $isShowingMesh)
                .toggleStyle(.switch)
                .frame(width:380)
                .padding(10)
                .onChange(of: isShowingMesh){ oldvalue, newValue in
                    CallCSharpCallback("showmesh")
                }
            if(true)
            {
                VStack {
                    Picker("Mesh Type", selection: $currentMeshViz) {
                        ForEach(MeshVisualization.allCases, id: \.self) {
                            Text($0.rawValue)
                        }
                    }
                    .pickerStyle(SegmentedPickerStyle())
                    .frame(width: 380)
                    .glassBackgroundEffect()
                    .disabled(!isShowingMesh)
                    .onReceive([self.$currentMeshViz].publisher.first(), perform: { _ in
                        SetSelectedMat(materialType: currentMeshViz)
                    })
                }
                
            }
            Toggle("Spawn Objects", isOn: $isSpawningObjects)
                .toggleStyle(.switch)
                .frame(width:380)
                .padding()
                .onChange(of: isSpawningObjects){
                    CallCSharpCallback("spawnObjects")
                }
            
            HStack {
                
                Button("Delete Objects"){
                    CallCSharpCallback("deleteObjects")
                }

                Spacer()
                
                Button("Return to Menu"){
                    CallCSharpCallback("returnToMenu")
                }
            }
            .frame(width:380)
            
        }
        
        .onAppear {
            // Call the public function that was defined in SwiftUISamplePlugin
            // inside UnityFramework
            CallCSharpCallback("appeared")
        }
    }
}

func SetSelectedMat(materialType: MeshVisualization) {
    switch materialType {
    case .occlusion:
        CallCSharpCallback("occlusionMat")
    case .texture:
        CallCSharpCallback("textureMat")
    case .WireFrame:
        CallCSharpCallback("wireframeMat")
    }
}

enum MeshVisualization: String, CaseIterable {
    case occlusion = "Occlusion"
    case texture = "Texture"
    case WireFrame = "WireFrame"
}

#Preview(windowStyle: .automatic) {
    MeshDemoContentView()
}

