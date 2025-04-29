using UnityEngine;

public interface ISaveable
{
    object CaptureState();           // Veriyi kaydetmek için
    void RestoreState(object state); // Veriyi geri yüklemek için
    string GetUniqueIdentifier();    // Bu nesneye özel bir ID
}

