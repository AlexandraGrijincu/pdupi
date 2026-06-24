import { styles } from './styles';
import React, { useState, useRef, useEffect } from 'react';
import {
  StyleSheet,
  Text,
  View,
  Dimensions,
  TextInput,
  TouchableOpacity,
  StatusBar,
  Alert,
  KeyboardAvoidingView,
  Platform,
  Animated,
  ScrollView,
  ImageBackground
} from 'react-native';
import Svg, { Path, Line } from 'react-native-svg';

// IMPORT EXPO PENTRU GALERIA REALĂ
import * as ImagePicker from 'expo-image-picker';

import { API_URLS } from './constants';

const { height, width } = Dimensions.get('window');

const HEADER_MAX_HEIGHT = height;
const HEADER_MIN_HEIGHT = 220;
const SCROLL_DISTANCE = HEADER_MAX_HEIGHT - HEADER_MIN_HEIGHT;

const EyeIcon = ({ visible }: { visible: boolean }) => {
  return (
      <Svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="rgba(255, 24, 147, 0.6)" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
        {visible ? (
            <>
              <Path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z" />
              <Path d="M12 15a3 3 0 1 0 0-6 3 3 0 0 0 0 6z" />
            </>
        ) : (
            <>
              <Path d="M17.94 17.94A10.07 10.07 0 0 1 12 20c-7 0-11-8-11-8a18.45 18.45 0 0 1 5.06-5.94M9.9 4.24A9.12 9.12 0 0 1 12 4c7 0 11 8 11 8a18.5 18.5 0 0 1-2.16 3.19m-6.72-1.07a3 3 0 1 1-4.24-4.24" />
              <Line x1="1" y1="1" x2="23" y2="23" />
            </>
        )}
      </Svg>
  );
};

const sportImagesMapping: { [key: string]: any } = {
  pilatesyoga1: require('./assets/sportIMG/pilatesyoga1.jpeg'),
  zumba: require('./assets/sportIMG/zumba.jpeg'),
  general: [
    require('./assets/sportIMG/sport1.jpeg'),
    require('./assets/sportIMG/sport2.jpeg'),
    require('./assets/sportIMG/sport3.jpeg'),
    require('./assets/sportIMG/sport4.jpeg'),
    require('./assets/sportIMG/sport5.jpeg'),
    require('./assets/sportIMG/sport6.jpeg'),
  ],
  default: require('./assets/start.jpeg'),
};

const subscriptionTypesOptions = [
  { id: 1, name: 'Glow Bronze 🌸', price: '120 RON', desc: '1 zi pe săptămână (4 Sesiuni/Lună)' },
  { id: 2, name: 'Glow Silver 💖', price: '180 RON', desc: '2 zile pe săptămână (8 Sesiuni/Lună)' },
  { id: 3, name: 'Glow Gold ✨', price: '240 RON', desc: '3 zile pe săptămână (12 Sesiuni/Lună)' },
  { id: 4, name: 'Glow Diamond Unlimited 👑', price: '350 RON', desc: 'Nelimitat într-o lună întreagă' },
];

const getSportImageByTitle = (sportName: string, classId: number) => {
  if (!sportName) return sportImagesMapping.general[0];
  const nameLower = sportName.toLowerCase().trim();

  if (nameLower.includes('zumba')) return sportImagesMapping.zumba;
  if (nameLower.includes('pilates') || nameLower.includes('yoga') || nameLower.includes('ioga')) return sportImagesMapping.pilatesyoga1;

  const index = Math.abs(classId) % sportImagesMapping.general.length;
  return sportImagesMapping.general[index];
};

const gymRoomsList = [
  { id: 1, name: 'Room 1 (F1)', capacity: 100 },
  { id: 2, name: 'Room 2 (F1)', capacity: 50 },
  { id: 3, name: 'Room 3 (F1)', capacity: 50 },
  { id: 4, name: 'Room 4 (F2)', capacity: 15 },
  { id: 5, name: 'Room 5 (F2)', capacity: 20 },
  { id: 6, name: 'Room 6 (F2)', capacity: 30 },
  { id: 7, name: 'Room 7 (F2)', capacity: 15 },
  { id: 8, name: 'Room 8 (F3)', capacity: 30 },
  { id: 9, name: 'Room 9 (F3)', capacity: 50 },
  { id: 10, name: 'Room 10 (F3)', capacity: 35 },
];

export default function App() {
  const [isLoggedIn, setIsLoggedIn] = useState<boolean>(false);
  const [currentUser, setCurrentUser] = useState<any>(null);
  const [currentView, setCurrentView] = useState<'login' | 'register' | 'confirmRegister' | 'forgot' | 'verifyCode'>('login');
  const [activeTab, setActiveTab] = useState<'home' | 'profile' | 'createClass' | 'scanQr'>('home');

  const [email, setEmail] = useState<string>('');
  const [password, setPassword] = useState<string>('');
  const [fullName, setFullName] = useState<string>('');
  const [inviteCode, setInviteCode] = useState<string>('');
  const [staffError, setStaffError] = useState<string>('');
  const [resetCode, setResetCode] = useState<string>('');
  const [newPassword, setNewPassword] = useState<string>('');
  const [registerVerifyCode, setRegisterVerifyCode] = useState<string>('');
  const [showPassword, setShowPassword] = useState<boolean>(false);
  const [showNewPassword, setShowNewPassword] = useState<boolean>(false);

  const [gymClasses, setGymClasses] = useState<any[]>([]);
  const [allBookings, setAllBookings] = useState<any[]>([]);
  const [categories, setCategories] = useState<string[]>(['All']);
  const [searchQuery, setSearchQuery] = useState<string>('');
  const [selectedCategory, setSelectedCategory] = useState<string>('All');
  const [expandedClassId, setExpandedClassId] = useState<number | null>(null);

  const [profileImageSource, setProfileImageSource] = useState<any>(require('./assets/start.jpeg'));

  const [createSportName, setCreateSportName] = useState<string>('');
  const [selectedRoomId, setSelectedRoomId] = useState<number>(1);
  const [createMaxParticipants, setCreateMaxParticipants] = useState<string>('');
  const [createDate, setCreateDate] = useState<string>('');
  const [createTime, setCreateTime] = useState<string>('');
  const [createDuration, setCreateDuration] = useState<string>('');
  const [qrSimulatedToken, setQrSimulatedToken] = useState<string>('');

  const scrollY = useRef(new Animated.Value(0)).current;
  const scrollViewRef = useRef<any>(null);

  const headerHeight = scrollY.interpolate({ inputRange: [0, SCROLL_DISTANCE], outputRange: [HEADER_MAX_HEIGHT, HEADER_MIN_HEIGHT], extrapolate: 'clamp' });
  const textOpacity = scrollY.interpolate({ inputRange: [0, SCROLL_DISTANCE / 2], outputRange: [1, 0], extrapolate: 'clamp' });
  const textTranslateY = scrollY.interpolate({ inputRange: [0, SCROLL_DISTANCE], outputRange: [0, -100], extrapolate: 'clamp' });

  const navigateToView = (view: 'login' | 'register' | 'confirmRegister' | 'forgot' | 'verifyCode') => {
    setEmail(''); setPassword(''); setFullName(''); setInviteCode(''); setStaffError('');
    setResetCode(''); setNewPassword(''); setRegisterVerifyCode(''); setShowPassword(false); setShowNewPassword(false);
    setCurrentView(view);
  };

  const isValidEmail = (text: string): boolean => {
    return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(text.trim());
  };

  useEffect(() => {
    if (isLoggedIn) {
      loadDashboardData().catch((err: any) => console.log(err));

      if (currentUser?.ProfileImage) {
        setProfileImageSource({ uri: `data:image/jpeg;base64,${currentUser.ProfileImage}` });
      } else {
        setProfileImageSource(require('./assets/start.jpeg'));
      }
    }
  }, [isLoggedIn, currentUser]);

  const loadDashboardData = async () => {
    await fetchGymClasses();
    await fetchAllBookingsDirect();
  };

  const fetchGymClasses = async () => {
    try {
      const response = await fetch(API_URLS.ODATA_GYM_CLASSES, {
        method: 'GET',
        headers: { 'Accept': 'application/json', 'Authorization': `Bearer ${currentUser?.Token}` }
      });
      if (!response.ok) return;
      const textData = await response.text();
      if (!textData) return;
      const data = JSON.parse(textData);
      if (data && data.value) {
        setGymClasses(data.value);
        const acum = new Date();
        const activeSports = data.value
            .filter((c: any) => new Date(c.StartTime).getTime() > acum.getTime())
            .map((c: any) => c.SportType?.Name)
            .filter((name: string, index: number, self: string[]) => name && self.indexOf(name) === index);

        setCategories(['All', ...activeSports]);
      }
    } catch (error: any) {
      console.log("Eroare clase:", error);
    }
  };

  const fetchAllBookingsDirect = async () => {
    try {
      const response = await fetch(API_URLS.ALL_BOOKINGS, {
        method: 'GET',
        headers: { 'Accept': 'application/json', 'Authorization': `Bearer ${currentUser?.Token}` }
      });
      if (response.ok) {
        const data = await response.json();
        setAllBookings(data);
      }
    } catch (error: any) {
      console.log("Eroare rezervari:", error);
    }
  };

  const handleLogin = async () => {
    if (!email || !password) { Alert.alert("Glow Alert 🌸", "Enter your credentials! ✨"); return; }
    try {
      const response = await fetch(API_URLS.LOGIN, { method: 'POST', headers: { 'Accept': 'application/json', 'Content-Type': 'application/json' }, body: JSON.stringify({ Email: email.trim().toLowerCase(), Password: password }) });
      const data = await response.json();
      if (response.ok && data && data.value && data.value.length > 0) {
        const loggedUser = {
          ...data.value[0],
          Token: data.Token || data.value[0].Token,
          SubscriptionType: data.value[0].SubscriptionType || null,
          SessionsLeftThisWeek: data.value[0].SessionsLeftThisWeek ?? null
        };
        setCurrentUser(loggedUser);
        setIsLoggedIn(true);
        setActiveTab('home');
      } else { Alert.alert("Glow Info 🎀", "Invalid email or password. ✨"); }
    } catch (e: any) { Alert.alert("Network Info 🎀", "Server offline. ✨"); }
  };

  const handleRequestRegister = async () => {
    setStaffError('');
    if (!email || !password || !fullName) { Alert.alert("Glow Info 🌸", "Fill in all fields! 💕"); return; }
    if (!isValidEmail(email)) { Alert.alert("Glow Info 🌸", "Enter a valid address! ✨"); return; }
    if (inviteCode.trim() !== "" && inviteCode.trim() !== "GLOW_STAFF_2026") { setStaffError("The code is incorrect! 🌸"); return; }
    try {
      const response = await fetch(API_URLS.REQUEST_REGISTER, { method: 'POST', headers: { 'Accept': 'application/json', 'Content-Type': 'application/json' }, body: JSON.stringify({ Email: email.trim() }) });
      if (response.ok) { Alert.alert("Verify Email 🌸", "We sent a code! 💕", [{ text: "Enter Code 🔑", onPress: () => setCurrentView('confirmRegister') }]); }
    } catch (e: any) { console.log(e); }
  };

  const handleConfirmRegister = async () => {
    if (!registerVerifyCode) return;
    let assignedRole = inviteCode.trim() === "GLOW_STAFF_2026" ? 1 : 0;
    try {
      const response = await fetch(API_URLS.REGISTER, { method: 'POST', headers: { 'Accept': 'application/json', 'Content-Type': 'application/json' }, body: JSON.stringify({ FullName: fullName.trim(), Email: email.trim().toLowerCase(), Password: password, Role: assignedRole, Code: registerVerifyCode.trim() }) });
      if (response.ok) { Alert.alert("Welcome! 🌸", "Account ready! ✨"); navigateToView('login'); }
    } catch (e: any) { Alert.alert("Glow Info 🎀", "Registration failed. 💕"); }
  };

  const handleForgotPassword = async () => {
    if (!email || !isValidEmail(email)) return;
    try {
      const response = await fetch(API_URLS.RESET_PASSWORD, { method: 'POST', headers: { 'Accept': 'application/json', 'Content-Type': 'application/json' }, body: JSON.stringify({ Email: email.trim() }) });
      if (response.ok) { setCurrentView('verifyCode'); }
    } catch (e: any) { console.log(e); }
  };

  const handleVerifyAndReset = async () => {
    if (!resetCode || !newPassword) return;
    try {
      const response = await fetch(API_URLS.VERIFY_CODE, { method: 'POST', headers: { 'Accept': 'application/json', 'Content-Type': 'application/json' }, body: JSON.stringify({ Email: email.trim(), Code: resetCode.trim(), NewPassword: newPassword }) });
      if (response.ok) { navigateToView('login'); }
    } catch (e: any) { console.log(e); }
  };

  const handleBookPlace = async (classId: number) => {
    const userId = currentUser?.Id || currentUser?.id;

    // 1. DACA ESTI ANTRENOR (Role 1), doar deschizi/închizi lista de participanți
    if (currentUser?.Role === 1) {
      setExpandedClassId(expandedClassId === classId ? null : classId);
      return;
    }

    // 2. Logica pentru MEMBRI (Role 0)
    const dejaInscris = allBookings.some(
        (b: any) => (b.GymClassId === classId || b.gymClassId === classId) &&
            (b.MemberId === userId || b.memberId === userId || b.MemberName?.toLowerCase().trim() === currentUser.FullName?.toLowerCase().trim() || b.memberName?.toLowerCase().trim() === currentUser.FullName?.toLowerCase().trim())
    );

    if (dejaInscris) {
      Alert.alert("Loc Deja Rezervat! 💕", "Ești deja înscris la acest antrenament. GlowGym te așteaptă în sală! 🌸✨");
      return;
    }

    const areAbonamentValid = currentUser.SubscriptionType && currentUser.SubscriptionType.trim() !== "";
    const areCrediteDisponibile = currentUser.SessionsLeftThisWeek !== null && currentUser.SessionsLeftThisWeek > 0;

    if (!areAbonamentValid || !areCrediteDisponibile) {
      Alert.alert(
          "💳 Rezervare / Plată Ședință",
          !areAbonamentValid
              ? "Nu deții un abonament activ. Dorești să plătești separat o taxă unică de 35 RON pentru această ședință? ✨"
              : "Abonamentul tău a expirat sau creditele săptămânale s-au terminat! Dorești să plătești separat 35 RON? 💕",
          [
            { text: "Anulează", style: "cancel" },
            {
              text: "Plătește 35 RON 💳",
              onPress: async () => {
                try {
                  await fetch(API_URLS.PAY_PER_CLASS, {
                    method: 'POST',
                    headers: { 'Accept': 'application/json', 'Content-Type': 'application/json', 'Authorization': `Bearer ${currentUser?.Token}` },
                    body: JSON.stringify({ UserId: userId, ClassId: classId, userId: userId, classId: classId })
                  });
                  await executeBookingRequest(classId, userId);
                } catch(e: any) {
                  // Dacă plata eșuează din erori de rețea dar API-ul poate a procesat, încercăm rezervarea
                  await executeBookingRequest(classId, userId);
                }
              }
            }
          ]
      );
      return;
    }

    await executeBookingRequest(classId, userId);
  };

  const executeBookingRequest = async (classId: number, userId: number) => {
    try {
      const response = await fetch(API_URLS.BOOK_PLACE, {
        method: 'POST',
        headers: { 'Accept': 'application/json', 'Content-Type': 'application/json', 'Authorization': `Bearer ${currentUser?.Token}` },
        body: JSON.stringify({ ClassId: classId, MemberId: userId, classId: classId, memberId: userId })
      });

      if (response.ok) {
        let updatedSessionsLeft = currentUser.SessionsLeftThisWeek;
        let updatedSubName = currentUser.SubscriptionType;

        if (currentUser.SessionsLeftThisWeek !== null && currentUser.SessionsLeftThisWeek < 1000) {
          updatedSessionsLeft = currentUser.SessionsLeftThisWeek - 1;
          if (updatedSessionsLeft < 0) updatedSessionsLeft = 0;
        }

        if (updatedSessionsLeft === 0 && currentUser.SessionsLeftThisWeek !== null) {
          updatedSubName = null;
        }

        setCurrentUser({ ...currentUser, SessionsLeftThisWeek: updatedSessionsLeft, SubscriptionType: updatedSubName });

        const updatePayload = {
          UserId: userId, userId: userId,
          Id: userId, id: userId,
          NewCredits: updatedSessionsLeft, newCredits: updatedSessionsLeft,
          SessionsLeftThisWeek: updatedSessionsLeft, sessionsLeftThisWeek: updatedSessionsLeft,
          SubscriptionType: updatedSubName, subscriptionType: updatedSubName
        };

        if (updatedSessionsLeft === 0 && currentUser.SessionsLeftThisWeek !== null) {
          await fetch(`${API_URLS.BUY_SUBSCRIPTION}`, {
            method: 'POST',
            headers: { 'Accept': 'application/json', 'Content-Type': 'application/json', 'Authorization': `Bearer ${currentUser?.Token}` },
            body: JSON.stringify({ UserId: userId, SubscriptionTypeId: 0, userId: userId, subscriptionTypeId: 0 })
          });

          await fetch(`http://10.0.1.59:5218/api/Users/UpdateCredits`, {
            method: 'PUT',
            headers: { 'Accept': 'application/json', 'Content-Type': 'application/json', 'Authorization': `Bearer ${currentUser?.Token}` },
            body: JSON.stringify(updatePayload)
          });

        } else if (currentUser.SessionsLeftThisWeek !== null && currentUser.SessionsLeftThisWeek < 1000) {
          await fetch(`http://10.0.1.59:5218/api/Users/UpdateCredits`, {
            method: 'PUT',
            headers: { 'Accept': 'application/json', 'Content-Type': 'application/json', 'Authorization': `Bearer ${currentUser?.Token}` },
            body: JSON.stringify(updatePayload)
          });
        }

        await loadDashboardData();
        Alert.alert("Glow Success! ✨", "Loc rezervat cu succes! 🌸");
      }
    } catch (error: any) { console.log(error); }
  };

  const getZileRamase = () => {
    // 1. Verificăm dacă currentUser există și are ExpiryDate
    if (!currentUser || !currentUser.ExpiryDate) return "N/A";

    const dataExpirare = new Date(currentUser.ExpiryDate);
    const dataAzi = new Date();

    // Calculăm diferența în milisecunde
    const diferentaInMs = dataExpirare.getTime() - dataAzi.getTime();
    const zile = Math.ceil(diferentaInMs / (1000 * 60 * 60 * 24));

    // AICI E TRUCUL: Dacă 'zile' este greșit, înseamnă că dataExpirare e greșită.
    // Hai să vedem ce valoare are în consolă:
    console.log("DEBUG - ExpiryDate primit:", currentUser.ExpiryDate);
    console.log("DEBUG - Data calculată (Date):", dataExpirare);
    console.log("DEBUG - Zile calculate:", zile);

    return zile > 0 ? `${zile} zile rămase` : "Expirat";
  };

  const handleBuySubscriptionRequest = async (subOption: any) => {
    const userId = currentUser?.Id || currentUser?.id;
    const calculateSessions = subOption.id === 4 ? 9999 : (subOption.id === 1 ? 4 : subOption.id === 2 ? 8 : 12);
    try {
      const response = await fetch(API_URLS.BUY_SUBSCRIPTION, {
        method: 'POST',
        headers: { 'Accept': 'application/json', 'Content-Type': 'application/json', 'Authorization': `Bearer ${currentUser?.Token}` },
        body: JSON.stringify({ UserId: userId, SubscriptionTypeId: subOption.id, userId: userId, subscriptionTypeId: subOption.id })
      });

      if (response.ok) {
        const data = await response.json();
        console.log("Date primite de la server:", data);
        Alert.alert("Succes 💳", `Abonamentul ${subOption.name} a fost activat! 💕`);

        // Actualizezi utilizatorul cu datele primite de la server
        setCurrentUser({
          ...currentUser,
          SubscriptionType: subOption.name,
          SessionsLeftThisWeek: calculateSessions,
          ExpiryDate: data.expiryDate // <--- Data primită din noul cod de controller
        });
      }
    } catch (e: any) { console.log(e); }
  };

  const handleOpenNativePhoneGallery = async () => {
    const permissionResult = await ImagePicker.requestMediaLibraryPermissionsAsync();
    if (permissionResult.granted === false) return;

    const result = await ImagePicker.launchImageLibraryAsync({
      mediaTypes: 'images',
      allowsEditing: true,
      aspect: [1, 1],
      quality: 0.4,
      base64: true,
    });

    if (!result.canceled && result.assets && result.assets.length > 0) {
      const selectedAsset = result.assets[0];
      const base64String = selectedAsset.base64;
      const localUri = selectedAsset.uri;

      if (!base64String) return;

      const userId = currentUser?.Id || currentUser?.id;
      try {
        await fetch(API_URLS.UPDATE_PROFILE_IMAGE, {
          method: 'POST',
          headers: { 'Accept': 'application/json', 'Content-Type': 'application/json', 'Authorization': `Bearer ${currentUser?.Token}` },
          body: JSON.stringify({ UserId: userId, ProfileImage: base64String, userId: userId, profileImage: base64String })
        });
        setProfileImageSource({ uri: localUri });
        setCurrentUser({ ...currentUser, ProfileImage: base64String });
        Alert.alert("Glow Profile 💕", "Profile updated successfully! 🌸");
      } catch (error: any) { console.log(error); }
    }
  };

  const handleCreateClass = async () => {
    if (!createSportName || !createMaxParticipants || !createDate || !createTime || !createDuration) return;
    const combinedISOString = new Date(`${createDate.trim()}T${createTime.trim()}:00`).toISOString();
    try {
      const response = await fetch(API_URLS.CREATE_CLASS, {
        method: 'POST',
        headers: { 'Accept': 'application/json', 'Content-Type': 'application/json', 'Authorization': `Bearer ${currentUser?.Token}` },
        body: JSON.stringify({ TrainerId: currentUser.Id || currentUser.id, RoomId: selectedRoomId, SportTypeName: createSportName.trim(), MaxParticipants: parseInt(createMaxParticipants,10), Duration: parseInt(createDuration,10), StartTime: combinedISOString, trainerId: currentUser.Id || currentUser.id, roomId: selectedRoomId, sportTypeName: createSportName.trim(), maxParticipants: parseInt(createMaxParticipants,10), duration: parseInt(createDuration,10), startTime: combinedISOString })
      });
      if (response.ok) {
        Alert.alert("Class Created! 🌸", "Workout saved! ✨");
        setCreateSportName(''); setCreateMaxParticipants(''); setCreateDate(''); setCreateTime(''); setCreateDuration('');
        await loadDashboardData();
        setActiveTab('home');
      }
    } catch (error: any) { console.log(error); }
  };

  const handleSimulateQrScan = async () => {
    if (!qrSimulatedToken) return;
    try {
      const response = await fetch(`http://10.0.1.59:5218/api/Users/ScanQR`, {
        method: 'POST',
        headers: { 'Accept': 'application/json', 'Content-Type': 'application/json', 'Authorization': `Bearer ${currentUser?.Token}` },
        body: JSON.stringify({ QRToken: qrSimulatedToken.trim(), qrToken: qrSimulatedToken.trim() })
      });
      if (response.ok) { Alert.alert("Access Allowed! ✨", "Member valid! 🌸"); setQrSimulatedToken(''); }
    } catch (e: any) { console.log(e); }
  };

  const handleLogout = () => {
    setIsLoggedIn(false);
    setCurrentUser(null);
    setCurrentView('login');
    setActiveTab('home');
    setEmail('');
    setPassword('');
    setFullName('');
    setInviteCode('');
    setResetCode('');
    setRegisterVerifyCode('');
    setProfileImageSource(require('./assets/start.jpeg'));

    scrollY.setValue(0);
    if (scrollViewRef.current) {
      scrollViewRef.current.scrollTo({ y: 0, animated: false });
    }
  };

  if (isLoggedIn && currentUser) {
    const timeAcum = new Date().getTime();
    const filteredAndSortedClasses = gymClasses
        .filter(c => {
          const matchesSearch = c.SportType?.Name?.toLowerCase().includes(searchQuery.toLowerCase());
          const matchesCat = selectedCategory === 'All' || c.SportType?.Name === selectedCategory;
          return matchesSearch && matchesCat && new Date(c.StartTime).getTime() > timeAcum;
        })
        .sort((a, b) => new Date(a.StartTime).getTime() - new Date(b.StartTime).getTime());

    const myBookedClasses = gymClasses
        .filter(c => {
          return allBookings.some((b: any) => {
            if (b.GymClassId !== c.Id && b.gymClassId !== c.Id) return false;

            const matchId = (b.MemberId === currentUser.Id || b.memberId === currentUser.Id || b.MemberId === currentUser.id || b.memberId === currentUser.id);
            const matchName = b.MemberName?.toLowerCase().trim() === currentUser.FullName?.toLowerCase().trim() ||
                b.memberName?.toLowerCase().trim() === currentUser.FullName?.toLowerCase().trim();

            return matchId || matchName;
          }) && new Date(c.StartTime).getTime() > timeAcum;
        })
        .sort((a, b) => new Date(a.StartTime).getTime() - new Date(b.StartTime).getTime());

    const isTrainer = currentUser.Role === 1;

    const myCreatedClasses = gymClasses
        .filter(c => c.TrainerId === (currentUser.Id || currentUser.id) && new Date(c.StartTime).getTime() > timeAcum)
        .sort((a, b) => new Date(a.StartTime).getTime() - new Date(b.StartTime).getTime());



    return (
        <View style={styles.dashboardWrapper}>
          <StatusBar barStyle="light-content"/>

          {activeTab !== 'profile' && (
              <View style={styles.internalHeader}>
                <Text style={styles.internalLogo}>GlowGym</Text>
                <TouchableOpacity style={styles.logoutBtn} onPress={handleLogout}>
                  <Text style={styles.logoutBtnText}>Logout</Text>
                </TouchableOpacity>
              </View>
          )}

          {activeTab === 'home' && (
              <ScrollView contentContainerStyle={{paddingBottom: 110}} showsVerticalScrollIndicator={false}>
                <View style={styles.searchContainer}>
                  <TextInput style={styles.searchBarInput} placeholder="Search sport..." placeholderTextColor="#FF69B4"
                             value={searchQuery} onChangeText={setSearchQuery}/>
                  <Text style={styles.searchIconSymbol}>🌸</Text>
                </View>

                <View style={{height: 45, marginBottom: 15}}>
                  <ScrollView horizontal showsHorizontalScrollIndicator={false}
                              contentContainerStyle={{paddingHorizontal: 16, alignItems: 'center'}}>
                    {categories.map((cat) => (
                        <TouchableOpacity key={cat}
                                          style={[styles.categoryPill, selectedCategory === cat ? styles.categoryPillActive : null]}
                                          onPress={() => setSelectedCategory(cat)}>
                          <Text
                              style={[styles.categoryText, selectedCategory === cat ? styles.categoryTextActive : null]}>{cat}</Text>
                        </TouchableOpacity>
                    ))}
                  </ScrollView>
                </View>

                <View style={{paddingHorizontal: 16}}>
                  <Text style={styles.sectionTitle}>Workout Classes 💕</Text>
                  {filteredAndSortedClasses.map((item) => {
                    const totalClassBookings = allBookings.filter((b: any) => b.GymClassId === item.Id || b.gymClassId === item.Id);
                    const totalCount = totalClassBookings.length;

                    const isMyClass = item.TrainerId === (currentUser.Id || currentUser.id);
                    const attendingMembers = isMyClass ? totalClassBookings.map((b: any) => b.MemberName || b.memberName) : [];
                    const dynamicAvailableSlots = Math.max(0, item.MaxParticipants - totalCount);
                    const isRosterVisible = expandedClassId === item.Id;

                    const areAbonamentText = currentUser.SubscriptionType && currentUser.SubscriptionType.trim() !== "";
                    const areCrediteText = currentUser.SessionsLeftThisWeek !== null && currentUser.SessionsLeftThisWeek > 0;
                    const butonLabel = currentUser.Role === 0 ? ((!areAbonamentText || !areCrediteText) ? 'Pay 35 RON' : 'Book Place') : `👥 Participanți: ${totalCount} ${isRosterVisible ? '🔼' : '🔽'}`;

                    return (
                        <View key={item.Id} style={{marginBottom: 16}}>
                          <View style={styles.workoutCard}>
                            <ImageBackground source={getSportImageByTitle(item.SportType?.Name, item.Id)}
                                             style={styles.cardImageBackground} imageStyle={{borderRadius: 16}}>
                              <View style={styles.cardDarkOverlay}/>
                              <View style={styles.cardContentContainer}>
                                <View>
                                  <Text style={styles.cardSportTitle}>{item.SportType?.Name || "Workout Session"}</Text>
                                  <Text
                                      style={styles.cardSubDetails}>Trainer: {item.Trainer?.FullName || `ID ${item.TrainerId}`} •
                                    Room: {item.Room?.Name || `Room ${item.RoomId}`}</Text>
                                  <Text
                                      style={styles.cardTimeText}>📅 {new Date(item.StartTime).toLocaleDateString('ro-RO')} •
                                    🕒 {new Date(item.StartTime).toLocaleTimeString('ro-RO', {
                                      hour: '2-digit',
                                      minute: '2-digit'
                                    })}</Text>
                                </View>
                                <View style={styles.cardBottomRow}>
                                  <Text style={styles.slotsText}>Slots: {dynamicAvailableSlots} left</Text>
                                  <TouchableOpacity style={styles.bookButton} onPress={() => handleBookPlace(item.Id)}>
                                    <Text style={styles.bookButtonText}>{butonLabel}</Text>
                                  </TouchableOpacity>
                                </View>
                              </View>
                            </ImageBackground>
                          </View>

                          {currentUser.Role === 1 && isRosterVisible && (
                              <View style={styles.trainerRosterCard}>
                                {isMyClass ? (
                                    <>
                                      <Text style={styles.rosterTitle}>✨ Listă Participanți Înscriși
                                        ({totalCount}):</Text>
                                      {totalCount === 0 ? (
                                          <Text style={styles.rosterEmptyText}>Nu s-a înscris niciun participant încă.
                                            💕</Text>
                                      ) : (
                                          attendingMembers.map((name: string, idx: number) => (
                                              <Text key={idx} style={styles.rosterMemberName}>💖 {name}</Text>
                                          ))
                                      )}
                                    </>
                                ) : (
                                    <Text style={[styles.rosterEmptyText, {color: '#FF1493', fontWeight: 'bold'}]}>
                                      🔒 Listă Confidențială (Clasa aparține altui antrenor)
                                    </Text>
                                )}
                              </View>
                          )}
                        </View>
                    );
                  })}
                </View>
              </ScrollView>
          )}

          {activeTab === 'profile' && (
              <View style={styles.profileMainContainer}>
                <ImageBackground source={profileImageSource} style={StyleSheet.absoluteFillObject} resizeMode="cover">
                  <View style={styles.profileDarkOverlayGraduation}/>

                  <View style={styles.profileGlassFloatCard}>
                    <View style={styles.profileGlassHeaderRow}>
                      <View>
                        <Text style={styles.profileGlassName}>{currentUser.FullName}</Text>
                        <Text style={styles.profileGlassSubTag}>
                          {currentUser.SubscriptionType && currentUser.SessionsLeftThisWeek > 0 ? `👑 VIP - ${currentUser.SubscriptionType}` : "⚠️ Fără Abonament Activ"}
                        </Text>
                      </View>
                      <TouchableOpacity style={styles.profileUploadRealButton} onPress={handleOpenNativePhoneGallery}>
                        <Text style={{fontSize: 16}}>📷</Text>
                      </TouchableOpacity>
                    </View>

                    <View style={styles.profileStatsRowTable}>
                      <View style={styles.profileStatsItemBox}>
                        <Text style={styles.profileStatNumber}>
                          {isTrainer ? (
                              myCreatedClasses.length
                          ) : !currentUser.SubscriptionType || currentUser.SessionsLeftThisWeek <= 0 ? (
                              0
                          ) : (
                              currentUser.SessionsLeftThisWeek > 1000 ? "∞" : currentUser.SessionsLeftThisWeek
                          )}
                        </Text>
                        <Text style={styles.profileStatLabel}>{isTrainer ? "Clase Create" : "Credite"}</Text>
                      </View>

                      {!isTrainer && currentUser.SubscriptionType && currentUser.SessionsLeftThisWeek > 0 && (
                          <View style={styles.profileStatsItemBox}>
                            <Text style={[styles.profileStatNumber, {color: '#00E5FF'}]}>
                              {getZileRamase()}
                            </Text>
                            <Text style={styles.profileStatLabel}>Valabilitate</Text>
                          </View>
                      )}

                      <View style={styles.profileStatsItemBox}>
                        <Text style={styles.profileStatNumber}>{isTrainer ? 'Antrenor' : 'Membru'}</Text>
                        <Text style={styles.profileStatLabel}>Rol Club</Text>
                      </View>
                    </View>

                    {(!isTrainer && (!currentUser.SubscriptionType || currentUser.SubscriptionType.trim() === "" || currentUser.SessionsLeftThisWeek <= 0)) ? (
                        <View>
                          {myBookedClasses.length > 0 && (
                              <View style={{marginBottom: 15}}>
                                <Text style={styles.profileSectionInnerTitle}>Clasele Mele Active Programate 💕</Text>
                                <ScrollView style={{maxHeight: 75}} showsVerticalScrollIndicator={false}>
                                  {myBookedClasses.map((c) => (
                                      <View key={c.Id} style={styles.profileClassListRowStrip}>
                                        <Text style={styles.profileStripSportName}>✨ {c.SportType?.Name}</Text>
                                        <Text
                                            style={styles.profileStripTime}>📅 {new Date(c.StartTime).toLocaleDateString('ro-RO')} • {new Date(c.StartTime).toLocaleTimeString('ro-RO', {
                                          hour: '2-digit',
                                          minute: '2-digit'
                                        })}</Text>
                                      </View>
                                  ))}
                                </ScrollView>
                              </View>
                          )}
                          <Text style={styles.profileSectionInnerTitle}>Cumpără / Reînnoiește Abonament GlowGym 💳</Text>
                          <ScrollView horizontal showsHorizontalScrollIndicator={false}
                                      contentContainerStyle={{gap: 10, paddingVertical: 4}}>
                            {subscriptionTypesOptions.map((sub) => (
                                <TouchableOpacity key={sub.id} style={styles.buySubShopCardItem}
                                                  onPress={() => handleBuySubscriptionRequest(sub)}>
                                  <Text style={styles.shopSubTitleName}>{sub.name}</Text>
                                  <Text style={styles.shopSubPriceText}>{sub.price}</Text>
                                  <Text style={styles.shopSubDescText}>{sub.desc}</Text>
                                </TouchableOpacity>
                            ))}
                          </ScrollView>
                        </View>
                    ) : (
                        <View>
                          <Text style={styles.profileSectionInnerTitle}>
                            {isTrainer ? "Clasele mele active publicate 🏋️‍♂️" : "Clasele Mele Active Programate 💕"}
                          </Text>
                          <ScrollView style={{maxHeight: 110}} showsVerticalScrollIndicator={false}>
                            {isTrainer ? (
                                myCreatedClasses.map((c) => (
                                    <View key={c.Id} style={styles.profileClassListRowStrip}>
                                      <Text style={styles.profileStripSportName}>🏋️‍♂️ {c.SportType?.Name}</Text>
                                      <Text
                                          style={styles.profileStripTime}>🕒 {new Date(c.StartTime).toLocaleDateString('ro-RO')} • {new Date(c.StartTime).toLocaleTimeString('ro-RO', {
                                        hour: '2-digit',
                                        minute: '2-digit'
                                      })}</Text>
                                    </View>
                                ))
                            ) : (
                                myBookedClasses.map((c) => (
                                    <View key={c.Id} style={styles.profileClassListRowStrip}>
                                      <Text style={styles.profileStripSportName}>✨ {c.SportType?.Name}</Text>
                                      <Text
                                          style={styles.profileStripTime}>📅 {new Date(c.StartTime).toLocaleDateString('ro-RO')} •
                                        🕒 {new Date(c.StartTime).toLocaleTimeString('ro-RO', {
                                          hour: '2-digit',
                                          minute: '2-digit'
                                        })}</Text>
                                    </View>
                                ))
                            )}
                          </ScrollView>
                        </View>
                    )}

                    <TouchableOpacity style={styles.profileInlineLogoutMinimalButton} onPress={handleLogout}>
                      <Text style={styles.profileInlineLogoutMinimalText}>Secure Sign Out 🔒</Text>
                    </TouchableOpacity>
                  </View>
                </ImageBackground>
              </View>
          )}

          {activeTab === 'createClass' && currentUser.Role === 1 && (
              <ScrollView contentContainerStyle={{paddingHorizontal: 16, paddingBottom: 110}}>
                <View style={styles.staffFormCard}>
                  <Text style={styles.staffCardTitle}>Publish New Workout 🌸</Text>
                  <Text style={styles.staffCardSubtitle}>Welcome back, {currentUser.FullName} ✨</Text>
                  <Text style={styles.labelField}>Sport Name:</Text>
                  <TextInput placeholder="e.g., Zumba, Pilates, Functional..." style={styles.staffInput}
                             value={createSportName} onChangeText={setCreateSportName}
                             placeholderTextColor="rgba(255,182,193,0.4)"/>
                  <View style={{flexDirection: 'row', justifyContent: 'space-between'}}>
                    <View style={{flex: 1, marginRight: 4}}>
                      <Text style={styles.labelField}>Date:</Text>
                      <TextInput placeholder="YYYY-MM-DD" style={styles.staffInput} value={createDate}
                                 onChangeText={setCreateDate} placeholderTextColor="rgba(255,182,193,0.4)"/>
                    </View>
                    <View style={{flex: 1, marginHorizontal: 4}}>
                      <Text style={styles.labelField}>Time:</Text>
                      <TextInput placeholder="HH:MM" style={styles.staffInput} value={createTime}
                                 onChangeText={setCreateTime} placeholderTextColor="rgba(255,182,193,0.4)"/>
                    </View>
                    <View style={{flex: 1, marginLeft: 4}}>
                      <Text style={styles.labelField}>Duration (min):</Text>
                      <TextInput placeholder="e.g., 50" style={styles.staffInput} keyboardType="number-pad"
                                 value={createDuration} onChangeText={setCreateDuration}
                                 placeholderTextColor="rgba(255,182,193,0.4)"/>
                    </View>
                  </View>
                  <Text style={styles.labelField}>Select Gym Room:</Text>
                  <View style={{height: 50, marginBottom: 15}}>
                    <ScrollView horizontal showsHorizontalScrollIndicator={false}
                                contentContainerStyle={{alignItems: 'center'}}>
                      {gymRoomsList.map(r => (
                          <TouchableOpacity key={r.id}
                                            style={[styles.roomPill, selectedRoomId === r.id ? styles.roomPillActive : null]}
                                            onPress={() => setSelectedRoomId(r.id)}>
                            <Text style={{color: '#fff', fontSize: 13, fontWeight: 'bold'}}>✨ {r.name}</Text>
                          </TouchableOpacity>
                      ))}
                    </ScrollView>
                  </View>
                  <Text style={styles.labelField}>Maximum Participants Slots:</Text>
                  <TextInput placeholder="e.g., 20" style={styles.staffInput} keyboardType="number-pad"
                             value={createMaxParticipants} onChangeText={setCreateMaxParticipants}
                             placeholderTextColor="rgba(255,182,193,0.4)"/>
                  <TouchableOpacity style={styles.staffButton} onPress={handleCreateClass}>
                    <Text style={styles.staffButtonText}>Create & Sync Sport 💕</Text>
                  </TouchableOpacity>
                </View>
              </ScrollView>
          )}

          {activeTab === 'scanQr' && currentUser.Role === 1 && (
              <ScrollView contentContainerStyle={{paddingHorizontal: 16, paddingBottom: 110}}>
                <View style={[styles.staffFormCard, {borderColor: '#FF1493'}]}>
                  <Text style={[styles.staffCardTitle, {color: '#FF1493'}]}>QR Code Check-In ✨</Text>
                  <TextInput placeholder="Paste Member Access Token"
                             style={[styles.staffInput, {borderColor: 'rgba(255, 20, 147, 0.3)'}]}
                             value={qrSimulatedToken} onChangeText={setQrSimulatedToken} autoCapitalize="none"
                             placeholderTextColor="rgba(255,182,193,0.4)"/>
                  <TouchableOpacity style={[styles.staffButton, {backgroundColor: '#FF1493'}]}
                                    onPress={handleSimulateQrScan}>
                    <Text style={[styles.staffButtonText, {color: '#fff'}]}>Verify Member Access 🌸</Text>
                  </TouchableOpacity>
                </View>
              </ScrollView>
          )}

          <View style={styles.bottomTabBar}>
            <TouchableOpacity style={[styles.tabItem, activeTab === 'home' ? styles.tabItemActive : null]}
                              onPress={() => setActiveTab('home')}>
              <Text style={styles.tabIcon}>🌸</Text>
              <Text style={[styles.tabLabel, activeTab === 'home' ? styles.tabLabelActive : null]}>Home</Text>
            </TouchableOpacity>

            <TouchableOpacity style={[styles.tabItem, activeTab === 'profile' ? styles.tabItemActive : null]}
                              onPress={() => setActiveTab('profile')}>
              <Text style={styles.tabIcon}>👑</Text>
              <Text style={[styles.tabLabel, activeTab === 'profile' ? styles.tabLabelActive : null]}>Profile</Text>
            </TouchableOpacity>

            {currentUser.Role === 1 && (
                <>
                  <TouchableOpacity style={[styles.tabItem, activeTab === 'createClass' ? styles.tabItemActive : null]}
                                    onPress={() => setActiveTab('createClass')}>
                    <Text style={styles.tabIcon}>💕</Text>
                    <Text
                        style={[styles.tabLabel, activeTab === 'createClass' ? styles.tabLabelActive : null]}>Create</Text>
                  </TouchableOpacity>
                  <TouchableOpacity style={[styles.tabItem, activeTab === 'scanQr' ? styles.tabItemActive : null]}
                                    onPress={() => setActiveTab('scanQr')}>
                    <Text style={styles.tabIcon}>✨</Text>
                    <Text style={[styles.tabLabel, activeTab === 'scanQr' ? styles.tabLabelActive : null]}>Scan
                      Access</Text>
                  </TouchableOpacity>
                </>
            )}
          </View>
        </View>
    );
  }
  return (
      <View style={styles.mainWrapper}>
        <StatusBar barStyle="light-content" translucent backgroundColor="transparent" />
        <Animated.View style={[styles.headerContainer, { height: headerHeight }]} pointerEvents="none">
          <Animated.Image source={require('./assets/start.jpeg')} style={styles.headerImage} resizeMode="cover" />
          <View style={styles.darkOverlayImage} />
          <Animated.View style={[styles.headerOverlayContent, { opacity: textOpacity, transform: [{ translateY: textTranslateY }] }]}>
            <Text style={styles.logoTextStart}>GlowGym</Text>
            <View style={styles.swipeIndicator}>
              <Text style={styles.swipeText}>Swipe Up to start 🌸</Text>
              <Text style={styles.arrow}>︾</Text>
            </View>
          </Animated.View>
        </Animated.View>

        <Animated.ScrollView
            ref={scrollViewRef}
            contentContainerStyle={styles.scrollContent}
            showsVerticalScrollIndicator={false}
            scrollEventThrottle={16}
            decelerationRate="fast"
            snapToInterval={SCROLL_DISTANCE}
            snapToAlignment="start"
            bounces={false}
            onScroll={Animated.event([{ nativeEvent: { contentOffset: { y: scrollY } } }], { useNativeDriver: false })}
        >
          <View style={{ height: HEADER_MAX_HEIGHT }} />
          <View style={styles.formContainer}>
            <KeyboardAvoidingView behavior={Platform.OS === "ios" ? "padding" : undefined} style={styles.keyboardView}>
              <View style={styles.glassCard}>
                <Text style={styles.loginTitle}>
                  {currentView === 'login' && 'Glow Account 💕'}
                  {currentView === 'register' && 'Join the Club 🌸'}
                  {currentView === 'confirmRegister' && 'Activate Account ✨'}
                  {currentView === 'forgot' && 'Reset Password 🎀'}
                  {currentView === 'verifyCode' && 'Enter Secure Code 🔑'}
                </Text>
                <View style={styles.form}>
                  {currentView === 'register' && <TextInput placeholder="Enter Full Name" style={styles.input} onChangeText={setFullName} placeholderTextColor="rgba(255,182,193,0.4)" value={fullName} />}
                  {currentView !== 'verifyCode' && currentView !== 'confirmRegister' && <TextInput placeholder="Enter Email" style={styles.input} onChangeText={setEmail} autoCapitalize="none" placeholderTextColor="rgba(255,182,193,0.4)" keyboardType="email-address" value={email} />}
                  {currentView !== 'forgot' && currentView !== 'verifyCode' && currentView !== 'confirmRegister' && (
                      <View style={styles.passwordInputContainer}>
                        <TextInput placeholder="Enter Password" style={styles.passwordInputField} onChangeText={setPassword} secureTextEntry={!showPassword} autoCapitalize="none" placeholderTextColor="rgba(255,182,193,0.4)" value={password} />
                        <TouchableOpacity style={styles.eyeButton} onPress={() => setShowPassword(!showPassword)}><EyeIcon visible={showPassword} /></TouchableOpacity>
                      </View>
                  )}
                  {currentView === 'confirmRegister' && <TextInput placeholder="Enter Code" style={styles.input} onChangeText={setRegisterVerifyCode} keyboardType="number-pad" maxLength={4} placeholderTextColor="rgba(255,182,193,0.4)" value={registerVerifyCode} />}
                  {currentView === 'verifyCode' && (
                      <View>
                        <TextInput placeholder="Enter Code" style={styles.input} onChangeText={setResetCode} keyboardType="number-pad" maxLength={4} placeholderTextColor="rgba(255,182,193,0.4)" value={resetCode} />
                        <View style={styles.passwordInputContainer}>
                          <TextInput placeholder="Enter New Password" style={styles.passwordInputField} onChangeText={setNewPassword} secureTextEntry={!showNewPassword} autoCapitalize="none" placeholderTextColor="rgba(255,182,193,0.4)" value={newPassword} />
                          <TouchableOpacity style={styles.eyeButton} onPress={() => setShowNewPassword(!showNewPassword)}><EyeIcon visible={showNewPassword} /></TouchableOpacity>
                        </View>
                      </View>
                  )}
                  {currentView === 'login' && (
                      <TouchableOpacity style={styles.forgotPasswordContainer} onPress={() => navigateToView('forgot')}>
                        <Text style={styles.forgotPasswordText}>Forgot my Password 🎀</Text>
                      </TouchableOpacity>
                  )}
                  {currentView === 'register' && (
                      <View>
                        <TextInput placeholder="Staff Code (Optional)" style={[styles.input, staffError ? styles.inputError : null]} onChangeText={(text) => { setInviteCode(text); setStaffError(''); }} placeholderTextColor="rgba(255, 182, 193, 0.4)" autoCapitalize="none" value={inviteCode} />
                        {staffError ? <Text style={styles.errorText}>{staffError}</Text> : null}
                      </View>
                  )}
                  <TouchableOpacity style={styles.mainButton} onPress={currentView === 'login' ? handleLogin : currentView === 'register' ? handleRequestRegister : currentView === 'confirmRegister' ? handleConfirmRegister : currentView === 'forgot' ? handleForgotPassword : handleVerifyAndReset}>
                    <Text style={styles.buttonText}>
                      {currentView === 'login' && 'Login ✨'}
                      {currentView === 'register' && 'Sign up 🌸'}
                      {currentView === 'confirmRegister' && 'Activate Account ✨'}
                      {currentView === 'forgot' && 'Send Reset Code 💕'}
                      {currentView === 'verifyCode' && 'Update Password 🎀'}
                    </Text>
                  </TouchableOpacity>
                  <View style={styles.dividerContainer}>
                    <View style={styles.line} />
                    <Text style={styles.orText}>🌸</Text>
                    <View style={styles.line} />
                  </View>
                  <TouchableOpacity style={styles.toggleLink} onPress={() => { if (currentView === 'forgot' || currentView === 'verifyCode' || currentView === 'confirmRegister') navigateToView('login'); else navigateToView(currentView === 'login' ? 'register' : 'login'); }}>
                    <Text style={styles.toggleText}>
                      {currentView === 'login' && "Don't have an account? Join us 🌸"}
                      {currentView === 'register' && "Already have an account? Login 💕"}
                      {currentView === 'confirmRegister' && "Back to Login ✨"}
                      {currentView === 'forgot' && "Back to Login ✨"}
                      {currentView === 'verifyCode' && "Back to Login ✨"}
                    </Text>
                  </TouchableOpacity>
                </View>
              </View>
            </KeyboardAvoidingView>
          </View>
        </Animated.ScrollView>
      </View>
  );
}

