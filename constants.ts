// constants.ts
const BASE_IP = "192.168.245.110";
const PORT = "5218";

export const API_URLS = {
    REQUEST_REGISTER: `http://${BASE_IP}:${PORT}/api/Users/RequestRegister`,
    REGISTER: `http://${BASE_IP}:${PORT}/api/Users/Register`,
    LOGIN: `http://${BASE_IP}:${PORT}/api/Users/Login`,
    RESET_PASSWORD: `http://${BASE_IP}:${PORT}/api/Users/ForgotPassword`,
    VERIFY_CODE: `http://${BASE_IP}:${PORT}/api/Users/VerifyResetCode`,
    SCAN_QR: `http://${BASE_IP}:${PORT}/api/Users/ScanQR`, // 🌟 Adăugat pentru scanner-ul antrenorului

    // 🌟 ADRESELE NOI PENTRU JOCUL CU CLASE ȘI REZERVĂRI
    ODATA_GYM_CLASSES: `http://${BASE_IP}:${PORT}/odata/GymClasses?$expand=Trainer,Room,SportType`,
    CREATE_CLASS: `http://${BASE_IP}:${PORT}/odata/GymClasses`,
    BOOK_PLACE: `http://${BASE_IP}:${PORT}/api/Bookings/BookPlace`,
    ALL_BOOKINGS: `http://${BASE_IP}:${PORT}/api/Bookings/AllWithMembers`,
    UPDATE_PROFILE_IMAGE: `http://${BASE_IP}:${PORT}/api/Users/UpdateProfileImage`,
    BUY_SUBSCRIPTION: `http://${BASE_IP}:${PORT}/api/Users/BuySubscription`,
    PAY_PER_CLASS: `http://${BASE_IP}:${PORT}/api/Users/PayPerClass`,
};