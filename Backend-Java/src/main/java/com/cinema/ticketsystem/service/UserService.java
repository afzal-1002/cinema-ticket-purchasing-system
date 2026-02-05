package com.cinema.ticketsystem.service;

import com.cinema.ticketsystem.dto.*;
import com.cinema.ticketsystem.model.User;

import java.util.List;

public interface UserService {
    AuthResponse register(RegisterRequest request);
    AuthResponse login(LoginRequest request);
    List<UserDTO> getAllUsers();
    UserDTO getUserById(Long id);
    User getUserEntityById(Long id);
    UserDTO updateUser(Long id, UpdateUserRequest request);
    void deleteUser(Long id);
    UserDTO getUserByEmail(String email);
}
