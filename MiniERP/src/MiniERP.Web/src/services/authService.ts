import api from './api';

export interface LoginRequest {
  email: string;
  password: string;
}

export interface AuthResult {
  success: boolean;
  token?: string;
  userId?: string;
  email?: string;
  fullName?: string;
  roles?: string[];
  errorMessage?: string;
}

export const authService = {
  login: async (credentials: LoginRequest): Promise<AuthResult> => {
    const payload = { Username: credentials.email, Password: credentials.password };
    // Debug helpers: save last request so user can paste it for troubleshooting
    try {
      localStorage.setItem('lastAuthRequest', JSON.stringify(payload));
    } catch {}
    try {
      console.debug('[authService] sending login payload', payload);
      const response = await api.post('/auth/login', payload);
      try { localStorage.setItem('lastAuthResponse', JSON.stringify(response.data)); } catch {}
      console.debug('[authService] received login response', response.data);
      // Backend returns { Token, Username, Role }
      const data = response.data as any;
      const result: AuthResult = {
        success: true,
        token: data.token ?? data.Token,
        email: data.Username ?? data.username,
        roles: data.Role ? [data.Role] : undefined,
      };

      if (result.token) {
        localStorage.setItem('token', result.token);
        localStorage.setItem('user', JSON.stringify(result));
      }

      return result;
    } catch (error: any) {
      try { localStorage.setItem('lastAuthError', JSON.stringify({ message: error?.message, response: error?.response?.data })); } catch {}
      const msg = error?.response?.data?.message || error?.message || 'Login error';
      console.debug('[authService] login error', msg, error?.response?.data);
      return { success: false, errorMessage: msg };
    }
  },

  logout: () => {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
  },

  getCurrentUser: (): AuthResult | null => {
    const userStr = localStorage.getItem('user');
    return userStr ? JSON.parse(userStr) : null;
  },

  isAuthenticated: (): boolean => {
    return !!localStorage.getItem('token');
  },
};
