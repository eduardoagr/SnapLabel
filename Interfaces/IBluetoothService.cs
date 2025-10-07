namespace SnapLabel.Interfaces {
    /// <summary>
    /// Contract for platform-specific Bluetooth service implementations.
    /// Supports scanning, connecting, data transfer, and disconnecting.
    /// </summary>
    public interface IBluetoothService {

        Task StartListeningAsync();


        /// <summary>
        /// Event raised when a Bluetooth device is discovered during scanning.
        /// </summary>
        event Action<BluetoothDeviceModel> DeviceFound;

        /// <summary>
        /// Event raised when the connection to a Bluetooth device is lost
        /// or manually disconnected.
        /// </summary>
        event Action DeviceDisconnected;

        /// <summary>
        /// Starts scanning for nearby Bluetooth devices.
        /// Triggers <see cref="DeviceFound"/> when new devices are found.
        /// </summary>
        void StartScan();

        /// <summary>
        /// Stops the current Bluetooth device scan.
        /// </summary>
        void StopScan();

        /// <summary>
        /// Attempts to establish a connection to a Bluetooth device
        /// using the provided device identifier.
        /// </summary>
        /// <param name="address">The device identifier or address used to connect.</param>
        /// <returns>True if the connection was successful; otherwise, false.</returns>
        Task<bool> ConnectAsync(string address);

        /// <summary>
        /// Sends raw byte data to the connected Bluetooth device.
        /// Intended for sending printer payloads, images, or commands.
        /// </summary>
        /// <param name="data">The byte array to send.</param>
        /// <returns>True if data was sent successfully; otherwise, false.</returns>
        Task<bool> SendDataAsync(byte[] data);

        /// <summary>
        /// Disconnects from the currently connected Bluetooth device.
        /// Also triggers the <see cref="DeviceDisconnected"/> event.
        /// </summary>
        Task Disconnect(string deviceI);

        /// <summary>
        /// Checks if Bluetooth is enabled on the device.
        /// </summary>
        Task<bool> IsBluetoothEnabledAsync();

        /// <summary>
        /// Attempts to read incoming data from the connected Bluetooth device.
        /// Intended for echo confirmation or printer response.
        /// </summary>
        /// <param name="expectedLength">Optional: number of bytes to read.</param>
        /// <returns>Byte array of received data, or null if nothing received.</returns>
    }
}
