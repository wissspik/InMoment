using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlaceFilter : MonoBehaviour
{
    public List<FilterContainer> filterContainers;

    public PlaceFinder placeFinder;

    public List<Place> filteredPlaces;
    public MapView mapView;
    public Action onFindedPlace;
    private void Start()
    {
        StartCoroutine(InitializePlaces());
    }
    private IEnumerator InitializePlaces()
    {
        while(mapView.center == null)
            yield return null;

        if (placeFinder.places == null || placeFinder.places.Count == 0)
            StartCoroutine(placeFinder.FindPlacesInRadius(mapView.center, 2));

        while (placeFinder.isFinding)
            yield return null;
        Debug.Log(placeFinder.places.Count);
    }
    public IEnumerator CheckPlacesInArea(float area)
    {
        while (placeFinder.isFinding)
            yield return null;
        Debug.Log($"Ïîèñê çàâåðø¸í, íàéäåíî {placeFinder.places.Count} çàâåäåíèé. " +
            $"Ïðèìåíÿåì ôèëüòðû...");
        if (ApplyFilters())
        {
            onFindedPlace?.Invoke();
            Debug.Log($"Çàâåäåíèå íàéäåíî â îáëàñòè {area} îò òåêóùåãî ìåñòîïîëîæåíèÿ");
            yield break;
        }
        else
        {
            float newArea = area + 0.5f;
            Debug.Log($"Íè îäíî çàâåäåíèå íå íàéäåíî. Íîâàÿ îáëàñòü: {area}");
            yield return new WaitForSeconds(10f);
            StartCoroutine(placeFinder.FindPlacesInRadius(mapView.center, newArea));
            StartCoroutine(CheckPlacesInArea(newArea));
        }
    }
    public bool ApplyFilters()
    {
        List<Place> places = placeFinder.places;

        foreach (var container in filterContainers)
        {
            var selectedFilters = container.filters
                .Where(f => f.isSelected)
                .Select(f => f.option)
                .ToArray();

            if (selectedFilters.Length == 0)
                continue;

            switch (container.type)
            {
                case FilterType.opening_hours:
                    places = places
                        .Where(p => selectedFilters.Contains(FilterOption.round_the_clock) && p.opening_hours == "24/7"
                        || selectedFilters.Contains(FilterOption.opened_now) && CheckTime(p.opening_hours))
                        .ToList();
                    break;

                case FilterType.contact:
                    places = places
                        .Where(p => selectedFilters.Contains(FilterOption.with_website) && !string.IsNullOrEmpty(p.website)
                        || selectedFilters.Contains(FilterOption.with_phone) && !string.IsNullOrEmpty(p.phone))
                        .ToList();
                    break;

                case FilterType.payment_method:
                    places = places
                        .Where(p =>
                            selectedFilters.Contains(FilterOption.payment_cash) && !string.IsNullOrEmpty(p.payment_cash) ||
                            selectedFilters.Contains(FilterOption.payment_card) && !string.IsNullOrEmpty(p.payment_credit_cards))
                        .ToList();
                    break;

                case FilterType.cuisine:
                    places = places
                        .Where(p => p.cuisine != null && p.cuisine.Any(c =>
                        {
                            if (Enum.TryParse<FilterOption>(c + "_cuisine", out var res))
                                return selectedFilters.Contains(res);
                            return false;
                        }))
                        .ToList();
                    break;

                case FilterType.other:
                    // Êàêèå-òî äðóãèå ôèëüòðû
                    break;
            }
        }
        if (places.Count > 0)
        {
            filteredPlaces = places;
            return true;
        }
        return false;
    }
    public void CancelFilters()
    {
        foreach (var container in filterContainers)
            foreach (var filter in container.filters)
                if(filter.isSelected)
                    filter.OnClick();
    }

    private bool CheckTime(string time)
    {
        return true; // Íóæíî íàïèñàòü ëîãèêó ïðîâåðêè òîãî,
                     // ÷òî òåêóùåå âðåìÿ âõîäèò âî âðåìÿ ðàáîòû çàâåäåíèÿ

        // ковбой ваняька, а ты в курсе что ApplyFilters у тебя всегда принимает отсюда true?
        // у тебя фильтрация не должна работать,и даже если работает,пиздец конченная практика так делать XDDDDDDDDDDDDDDDDDDDD
    }
}
