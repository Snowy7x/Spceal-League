using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

[Serializable]
public class Menu
{
    public string name;
    public GameObject menu;
    public Transform cameraPlace;
    public bool isSmooth;
    public bool isShowing;
}

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance;

    [SerializeField] private Camera cam;
    [SerializeField] List<Menu> menus;
    [SerializeField] string mainMenuName;
    [SerializeField] float transitionTime = 0.5f;
    private Menu _currentMenu;
    private Menu mainMenu;
    
    private void Awake()
    {
        Instance = this;
        mainMenu = menus.Find(menu => menu.name == mainMenuName);
        if (!cam) cam = Camera.main;
    }
    
    private void Start()
    {
        ShowMenu(mainMenu.name);
    }
    
    public void ShowMenu(string menuName)
    {
        if (_currentMenu != null)
        {
            _currentMenu.menu.SetActive(false);
            _currentMenu.isShowing = false;
        }
        
        _currentMenu = menus.Find(menu => menu.name == menuName);
        _currentMenu.menu.SetActive(true);
        _currentMenu.isShowing = true;
        // change camera position
        if (_currentMenu.cameraPlace != null)
        {
            StopAllCoroutines();
            if (_currentMenu.isSmooth)
            {
                StartCoroutine(SmoothCameraMove(_currentMenu.cameraPlace));
            }
            else
            {
                cam.transform.position = _currentMenu.cameraPlace.position;
                cam.transform.rotation = _currentMenu.cameraPlace.rotation;
            }
        }
    }
    
    
    IEnumerator SmoothCameraMove(Transform target)
    {
        while (Vector3.Distance(cam.transform.position, target.position) > 0.1f &&
               Quaternion.Angle(cam.transform.rotation, target.rotation) > 0.1f)
        {
            // pos
            cam.transform.position = Vector3.Lerp(cam.transform.position, target.position, Time.deltaTime * transitionTime);
            // rot
            cam.transform.rotation = Quaternion.Lerp(cam.transform.rotation, target.rotation, Time.deltaTime * transitionTime);
            yield return null;
        }
    }

    public void PlayGame(int mode = 1)
    {
        ShowMenu("Loading");
        CustomNetworkManager.Instance.CreateOrJoinLobby(mode);
    }
    
    public void QuitGame()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}
