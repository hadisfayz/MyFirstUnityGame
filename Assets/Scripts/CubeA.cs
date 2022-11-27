using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CubeA : MonoBehaviour {
    GameTrakInput input;

    // type for sphere coordinates
    public struct Sphere {
        public float r;
        public float theta;
        public float phi;
    }

    // type for cartesian coordinates
    public struct Cartesian {
        public float x;
        public float y;
        public float z;
    }
     // Awake is called when the script instance is being loaded
    void Awake() {
        input = new GameTrakInput();

        // Subscribe to the action and bind the callback
        // += adds callback function
        input.GameTrak.LeftXAxis.performed += ctx => RepositionCube();
        input.GameTrak.LeftYAxis.performed += ctx => RepositionCube();
        input.GameTrak.LeftZAxis.performed += ctx => RepositionCube();
    }

    // Reposition cube based on the game controller input
    void RepositionCube() {
        // get input values and convert to cartesian coordinates
        Cartesian cubeCoords = SphereToCartesian(GetJoystickInput());
        // set the position of the cube
        transform.position = new Vector3(cubeCoords.x, cubeCoords.y, cubeCoords.z);
    }

    Sphere GetJoystickInput() {
        Sphere sphereCoords = new Sphere();

        // spherical coordinates P(r, theta, phi)
        // r is the radius (distance from the origin)
        // theta is the angle from the x-axis (azimuthal angle) in the range [0, 2pi] (0 to 360 degrees)
        // phi is the angle from the z-axis (polar angle) in the range [0, pi] (0 to 180 degrees)
        sphereCoords.r = -input.GameTrak.LeftZAxis.ReadValue<float>(); // -1 to 1
        // r was inverted to make the cube move in the same direction as the controller
        sphereCoords.theta = input.GameTrak.LeftXAxis.ReadValue<float>(); // -1 to 1
        sphereCoords.phi = input.GameTrak.LeftYAxis.ReadValue<float>(); // -1 to 1

        // map the input values to the max values
        // we define r from -1 to 1 to 0 to 50 (could be from 0 to infinity)
        sphereCoords.r = (float)((sphereCoords.r+1) * 25f);
        // map theta from -1 to 1 to 0 to 360
        sphereCoords.theta = (float)((sphereCoords.theta+1) * 180f);
        // map phi from -1 to 1 to 0 to 180
        sphereCoords.phi = (float)((sphereCoords.phi+1) * 90f);

        return sphereCoords;
    }

    // Convert spherical coordinates to cartesian coordinates
    Cartesian SphereToCartesian(Sphere sphereCoords) {
        Cartesian cartCoords = new Cartesian();

        //convert degrees to radians
        sphereCoords.theta = (float)(sphereCoords.theta * Mathf.PI / 180f);
        sphereCoords.phi = (float)(sphereCoords.phi * Mathf.PI / 180f);

        cartCoords.x = (float)(sphereCoords.r * Mathf.Sin(sphereCoords.theta) * Mathf.Cos(sphereCoords.phi));
        cartCoords.y = (float)(sphereCoords.r * Mathf.Sin(sphereCoords.theta) * Mathf.Sin(sphereCoords.phi));
        cartCoords.z = (float)(sphereCoords.r * Mathf.Cos(sphereCoords.theta));

        return cartCoords;
    }

    // OnEnable is called when the script is enabled
    void OnEnable() {
        input.GameTrak.Enable();
    }

    // OnDisable is called when the script is disabled
    void OnDisable() {
        input.GameTrak.Disable();
    }
}
