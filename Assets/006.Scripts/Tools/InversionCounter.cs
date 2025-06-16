using System;

public static class InversionCounter
{
    public static int CountWithMergeSort(int[] array)
    {
        if (array == null || array.Length <= 1) return 0;
        int[] temp = new int[array.Length];
        return MergeSort(array, temp, 0, array.Length - 1);
    }

    private static int MergeSort(int[] array, int[] temp, int left, int right)
    {
        int inversions = 0;
        if (left < right)
        {
            int mid = (left + right) / 2;
            inversions += MergeSort(array, temp, left, mid); // 왼쪽 부분 배열 처리
            inversions += MergeSort(array, temp, mid + 1, right); // 오른쪽 부분 배열 처리
            inversions += Merge(array, temp, left, mid, right); // 부분 배열 merge, inversion 계산
        }
        return inversions;
    }

    private static int Merge(int[] array, int[] temp, int left, int mid, int right)
    {
        int i = left; // 왼쪽 부분 배열 시작 인덱스
        int j = mid + 1; // 오른쪽 부분 배열 시작 인덱스
        int k = left; // 임시 배열의 현재 위치
        int inversions = 0; // 현재 merge 단계 inversion 개수

        // 양쪽 부분 배열 비교하며 병합
        while (i <= mid && j <= right)
        {
            if (array[i] <= array[j])
                temp[k++] = array[i++]; // 왼쪽 요소가 작거나 같으면 그대로 삽입
            else
            {
                temp[k++] = array[j++]; // 오른쪽 요소가 더 작으면 삽입
                inversions += mid - i + 1; // inversion 카운트 증가
            }
        }

        // 남은 요소 처리
        while (i <= mid) temp[k++] = array[i++];
        while (j <= right) temp[k++] = array[j++];

        // 임시 배열 > 원본 배열 복사
        Array.Copy(temp, left, array, left, right - left + 1);

        return inversions;
    }
}

