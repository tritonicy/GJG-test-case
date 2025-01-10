using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Basically, this script is used to get the values of the sliders and set them to the GameManager script and start the game.
/// So player can manipulate the values of the sliders to change the grid size and the number of colors etc.
/// </summary>
public class SliderScript : MonoBehaviour
{
    [SerializeField] private Slider mSlider;
    [SerializeField] private Slider nSlider;
    [SerializeField] private Slider kSlider;
    [SerializeField] private Slider aSlider;
    [SerializeField] private Slider bSlider;
    [SerializeField] private Slider cSlider;
    [SerializeField] private TextMeshProUGUI mText;
    [SerializeField] private TextMeshProUGUI nText;
    [SerializeField] private TextMeshProUGUI kText;
    [SerializeField] private TextMeshProUGUI aText;
    [SerializeField] private TextMeshProUGUI bText;
    [SerializeField] private TextMeshProUGUI cText;
    [SerializeField] private Button button;
    private GameManager manager;


    private void Start()
    {
        manager = GameManager.Instance;
        mText.text = "Row (M): " + mSlider.value.ToString();
        nText.text = "Column (N): " + nSlider.value.ToString();
        kText.text = "Total Number of Colors (K): " + kSlider.value.ToString();
        aText.text = "A: " + aSlider.value.ToString();
        bText.text = "B: " + bSlider.value.ToString();
        cText.text = "C: " + cSlider.value.ToString();
    }
    public void OnMSliderValueChanged(float value)
    {
        mText.text = "Row (M): " + value.ToString();
    }

    public void OnNSliderValueChanged(float value)
    {
        nText.text = "Column (N): " + value.ToString();
    }

    public void OnKSliderValueChanged(float value)
    {
        kText.text = "Total Number of Colors (K): " + value.ToString();
    }

    public void OnASliderValueChanged(float value)
    {
        if (bSlider.value < value)
        {
            bSlider.value = value;
        }

        if (cSlider.value < value)
        {
            cSlider.value = value;
        }

        aText.text = "A: "+value.ToString();
    }

    public void OnBSliderValueChanged(float value)
    {
        if (cSlider.value < value)
        {
            cSlider.value = value;
        }


        if (value < aSlider.value)
        {
            aSlider.value = value;
        }

        bText.text ="B: " +value.ToString();
    }

    public void OnCSliderValueChanged(float value)
    {
        if (value < aSlider.value)
        {
            aSlider.value = value;
        }

        if (value < bSlider.value)
        {
            bSlider.value = value;
        }

        cText.text =  "C: "+ value.ToString();
    }

    public void OnStartButtonClicked()
    {
        manager.a = (int)aSlider.value;
        manager.b = (int)bSlider.value;
        manager.c = (int)cSlider.value;
        manager.m = (int)mSlider.value;
        manager.n = (int)nSlider.value;
        manager.k = (int)kSlider.value;


        // gameObject.GetComponentInParent<Canvas>().enabled = false;
        aSlider.gameObject.SetActive(false);
        bSlider.gameObject.SetActive(false);
        cSlider.gameObject.SetActive(false);
        mSlider.gameObject.SetActive(false);
        nSlider.gameObject.SetActive(false);
        kSlider.gameObject.SetActive(false);
        aText.gameObject.SetActive(false);
        bText.gameObject.SetActive(false);
        cText.gameObject.SetActive(false);
        mText.gameObject.SetActive(false);
        nText.gameObject.SetActive(false);
        kText.gameObject.SetActive(false);
        
        GridSystem.currentGrid.Initalize();

        button.gameObject.SetActive(false);
        
    }
}
