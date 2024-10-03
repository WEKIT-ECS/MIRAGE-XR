//
// This is a sample Swift plugin that provides an interface for
// the SwiftUI sample to interact with. It must be linked into
// UnityFramework, which is what the default Swift file plugin
// importer will place it into.
//
// It uses "@_cdecl", a Swift not-officially-supported attribute to
// provide C-style linkage and symbol for a given function.
//
// It also uses a "hack" to create an EnvironmentValues() instance
// in order to fetch the openWindow and dismissWindow action. Normally,
// these would be provided to a view via something like:
//
//    @Environment(\.openWindow) var openWindow
//
// but we don't have a view at this point, and it's expected that these
// actions will be global (and not view-specific) anyway.
//
// There are two additional files that complete this example:
// SwiftUISampleInjectedScene.swift and HelloWorldConventView.swift.
//
// Any file named "...InjectedScene.swift" will be moved to the Unity-VisionOS
// Xcode target (as it must be there in order to be referenced by the App), and
// its static ".scene" member will be added to the App's main Scene. See
// the comments in SwiftUISampleInjectedScene.swift for more information.
//
// Any file that's inside of a "SwiftAppSupport" directory anywhere in its path
// will also be moved to the Unity-VisionOS Xcode target. HelloWorldContentView.swift
// is inside SwiftAppSupport beceause it's needed by the WindowGroup this sample
// adds to provide its content.
//

import Foundation
import SwiftUI

// These methods are exported from Swift with an explicit C-style name using @_cdecl,
// to match what DllImport expects. You will need to do appropriate conversion from
// C-style argument types (including UnsafePointers and other friends) into Swift
// as appropriate.

// SetNativeCallback is called from the SwiftUIDriver MonoBehaviour in OnEnable,
// to give Swift code a way to make calls back into C#. You can use one callback or
// many, as appropriate for your application.
//
// Declared in C# as: delegate void CallbackDelegate(string command);
typealias CallbackDelegateType = @convention(c) (UnsafePointer<CChar>) -> Void
public typealias SetFPSDelegateType = (Float) -> Void

var callbackDelegate: CallbackDelegateType? = nil
var setFPSDelegate: SetFPSDelegateType? = nil
var sphereCount: Int = 0
var cubeCount: Int = 0

// Declared in C# as: static extern void SetNativeCallback(CallbackDelegate callback);
@_cdecl("SetNativeCallback")
func setNativeCallback(_ delegate: CallbackDelegateType)
{
    print("############ SET NATIVE CALLBACK")
    callbackDelegate = delegate
}

// This is a function for your own use from the enclosing Unity-VisionOS app, to call the delegate
// from your own windows/views (HelloWorldContentView uses this)
public func CallCSharpCallback(_ str: String)
{
    if (callbackDelegate == nil) {
        return
    }

    str.withCString {
        callbackDelegate!($0)
    }
}

// Declared in C# as: static extern void OpenSwiftUIWindow(string name);
@_cdecl("OpenSwiftUIWindow")
func openSwiftUIWindow(_ cname: UnsafePointer<CChar>)
{
    let openWindow = EnvironmentValues().openWindow

    let name = String(cString: cname)
    print("############ OPEN WINDOW \(name)")
    openWindow(id: name)
}

// Declared in C# as: static extern void CloseSwiftUIWindow(string name);
@_cdecl("CloseSwiftUIWindow")
func closeSwiftUIWindow(_ cname: UnsafePointer<CChar>)
{
    let dismissWindow = EnvironmentValues().dismissWindow

    let name = String(cString: cname)
    print("############ CLOSE WINDOW \(name)")
    dismissWindow(id: name)
}

// Declared in C# as: static extern void SetSphereCount(int count);
@_cdecl("SetSphereCount")
func setSphereCount(_ count: Int)
{
    sphereCount = count
}

// Declared in C# as: static extern void SetCubeCount(int count);
@_cdecl("SetCubeCount")
func setCubeCount(_ count: Int)
{
    cubeCount = count
}

// Called by button callbacks in HelloWorldContentView
public func GetCubeCount() -> Int {
    return cubeCount
}

// Called by button callbacks in HelloWorldContentView
public func GetSphereCount() -> Int {
    return sphereCount
}

// Declared in C# as: static extern void SetFPS(float fps);
@_cdecl("SetFPS")
func setFPS(_ fps: Float) {
    setFPSDelegate?(fps)
}

// ContentView should call this on appear, setting up a callback for when SwiftFPSCounter pushes new FPS values
// Only one ContentView is supported. Calling SubscribeToSetFPS multiple times will overwrite setFPSDelegate,
// so only the most recent caller will get FPS values.
public func SubscribeToSetFPS(setFPSMethod: @escaping SetFPSDelegateType) {
    setFPSDelegate = setFPSMethod
}
