package com.cinema.ticketsystem.service.implementation;

import com.cinema.ticketsystem.dto.*;
import com.cinema.ticketsystem.mapper.UserMapper;
import com.cinema.ticketsystem.model.User;
import com.cinema.ticketsystem.repository.UserRepository;
import com.cinema.ticketsystem.security.JwtTokenUtil;
import com.cinema.ticketsystem.service.UserService;
import jakarta.persistence.OptimisticLockException;
import lombok.RequiredArgsConstructor;
import org.springframework.security.authentication.AuthenticationManager;
import org.springframework.security.authentication.UsernamePasswordAuthenticationToken;
import org.springframework.security.core.userdetails.UserDetails;
import org.springframework.security.core.userdetails.UserDetailsService;
import org.springframework.security.crypto.password.PasswordEncoder;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

import java.util.List;
import java.util.stream.Collectors;

@Service
@RequiredArgsConstructor
public class UserServiceImpl implements UserService {
    
    private final UserRepository userRepository;
    private final PasswordEncoder passwordEncoder;
    private final JwtTokenUtil jwtTokenUtil;
    private final AuthenticationManager authenticationManager;
    private final UserDetailsService userDetailsService;
    private final UserMapper userMapper;
    
    @Transactional
    public AuthResponse register(RegisterRequest request) {
        if (userRepository.existsByEmail(request.getEmail())) {
            throw new RuntimeException("User with this email already exists");
        }
        
        User user = new User();
        user.setEmail(request.getEmail());
        user.setPassword(passwordEncoder.encode(request.getPassword()));
        user.setFirstName(request.getFirstName());
        user.setLastName(request.getLastName());
        user.setPhoneNumber(request.getPhoneNumber());
        user.setIsAdmin(false);
        
        User savedUser = userRepository.save(user);
        
        UserDetails userDetails = userDetailsService.loadUserByUsername(savedUser.getEmail());
        String token = jwtTokenUtil.generateToken(userDetails, savedUser.getId(), savedUser.getIsAdmin());
        
        return new AuthResponse(token, userMapper.toDTO(savedUser));
    }
    
    public AuthResponse login(LoginRequest request) {
        authenticationManager.authenticate(
            new UsernamePasswordAuthenticationToken(request.getEmail(), request.getPassword())
        );
        
        User user = userRepository.findByEmail(request.getEmail())
                .orElseThrow(() -> new RuntimeException("User not found"));
        
        UserDetails userDetails = userDetailsService.loadUserByUsername(user.getEmail());
        String token = jwtTokenUtil.generateToken(userDetails, user.getId(), user.getIsAdmin());
        
        return new AuthResponse(token, userMapper.toDTO(user));
    }
    
    public List<UserDTO> getAllUsers() {
        return userRepository.findAll().stream()
                .map(userMapper::toDTO)
                .collect(Collectors.toList());
    }
    
    public UserDTO getUserById(Long id) {
        @SuppressWarnings("null")
        User user = userRepository.findById(id)
                .orElseThrow(() -> new RuntimeException("User not found"));
        return userMapper.toDTO(user);
    }
    
    public UserDTO getUserByEmail(String email) {
        if (email == "") {
            throw new RuntimeException("Email cannot be empty") ;
        }
        User user = userRepository.findByEmail(email)
                .orElseThrow(() -> new RuntimeException("User not found"));
        return userMapper.toDTO(user);
    }
    
    @SuppressWarnings("null")
    public User getUserEntityById(Long id) {
        return userRepository.findById(id)
                .orElseThrow(() -> new RuntimeException("User not found"));
    }
    
    @Transactional
    public UserDTO updateUser(Long id, UpdateUserRequest request) {
        @SuppressWarnings("null")
        User user = userRepository.findById(id)
                .orElseThrow(() -> new RuntimeException("User not found"));
        
        // Set version for optimistic locking
        if (request.getVersion() != null && !request.getVersion().equals(user.getVersion())) {
            throw new OptimisticLockException("The user was modified by another user. Please reload and try again.");
        }
        
        user.setFirstName(request.getFirstName());
        user.setLastName(request.getLastName());
        user.setPhoneNumber(request.getPhoneNumber());
        
        try {
            User updatedUser = userRepository.save(user);
            return userMapper.toDTO(updatedUser);
        } catch (OptimisticLockException e) {
            throw new RuntimeException("The user was modified by another user. Please reload and try again.");
        }
    }
    
    @SuppressWarnings("null")
    @Transactional
    public void deleteUser(Long id) {
        User user = userRepository.findById(id)
                .orElseThrow(() -> new RuntimeException("User not found"));
        
        try {
            userRepository.delete(user);
        } catch (OptimisticLockException e) {
            throw new RuntimeException("The user was modified by another user. Please reload and try again.");
        }
    }
}
