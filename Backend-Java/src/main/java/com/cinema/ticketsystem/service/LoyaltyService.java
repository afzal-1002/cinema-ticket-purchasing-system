package com.cinema.ticketsystem.service;

import com.cinema.ticketsystem.model.*;
import com.cinema.ticketsystem.repository.*;
import lombok.RequiredArgsConstructor;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

import java.math.BigDecimal;
import java.time.LocalDate;
import java.util.*;
import java.util.stream.Collectors;

@Service
@RequiredArgsConstructor
public class LoyaltyService {
    
    private final LoyaltyAccountRepository loyaltyAccountRepository;
    private final LoyaltyTransactionRepository loyaltyTransactionRepository;
    private final DiscountRepository discountRepository;
    private final UserRepository userRepository;
    private final ReservationRepository reservationRepository;
    
    private static final int POINTS_PER_DOLLAR = 10;
    
    @Transactional
    public Map<String, Object> createLoyaltyAccount(Long userId) {
        @SuppressWarnings("null")
        User user = userRepository.findById(userId)
                .orElseThrow(() -> new RuntimeException("User not found"));
        
        if (loyaltyAccountRepository.existsByUserId(userId)) {
            throw new RuntimeException("Loyalty account already exists for this user");
        }
        
        LoyaltyAccount account = new LoyaltyAccount();
        account.setUser(user);
        account.setPoints(0);
        account.setLifetimePoints(0);
        account.setTier(LoyaltyAccount.MemberTier.BRONZE);
        
        LoyaltyAccount savedAccount = loyaltyAccountRepository.save(account);
        
        return convertAccountToMap(savedAccount);
    }
    
    @Transactional(readOnly = true)
    public Map<String, Object> getLoyaltyAccount(Long userId) {
        LoyaltyAccount account = loyaltyAccountRepository.findByUserId(userId)
                .orElseThrow(() -> new RuntimeException("Loyalty account not found"));
        
        return convertAccountToMap(account);
    }
    
    @Transactional
    public Map<String, Object> addPoints(Long userId, Long reservationId, BigDecimal amount) {
        @SuppressWarnings("null")
        LoyaltyAccount account = loyaltyAccountRepository.findByUserId(userId)
                .orElseGet(() -> {
                    LoyaltyAccount newAccount = new LoyaltyAccount();
                    newAccount.setUser(userRepository.findById(userId)
                            .orElseThrow(() -> new RuntimeException("User not found")));
                    newAccount.setPoints(0);
                    newAccount.setLifetimePoints(0);
                    newAccount.setTier(LoyaltyAccount.MemberTier.BRONZE);
                    return loyaltyAccountRepository.save(newAccount);
                });
        
        // Calculate points based on tier multiplier
        double multiplier = account.getTier().getPointsMultiplier();
        int basePoints = amount.multiply(BigDecimal.valueOf(POINTS_PER_DOLLAR)).intValue();
        int earnedPoints = (int) (basePoints * multiplier);
        
        account.setPoints(account.getPoints() + earnedPoints);
        account.setLifetimePoints(account.getLifetimePoints() + earnedPoints);
        
        // Update tier based on lifetime points
        LoyaltyAccount.MemberTier newTier = LoyaltyAccount.MemberTier.getTierByPoints(account.getLifetimePoints());
        boolean tierUpgraded = newTier != account.getTier();
        account.setTier(newTier);
        
        LoyaltyAccount updatedAccount = loyaltyAccountRepository.save(account);
        
        // Record transaction
        LoyaltyTransaction transaction = new LoyaltyTransaction();
        transaction.setLoyaltyAccount(updatedAccount);
        transaction.setPoints(earnedPoints);
        transaction.setType(LoyaltyTransaction.TransactionType.EARNED);
        transaction.setDescription("Earned " + earnedPoints + " points from ticket purchase");
        
        if (reservationId != null) {
            reservationRepository.findById(reservationId)
                    .ifPresent(transaction::setReservation);
        }
        
        loyaltyTransactionRepository.save(transaction);
        
        Map<String, Object> result = new HashMap<>();
        result.put("pointsEarned", earnedPoints);
        result.put("currentPoints", updatedAccount.getPoints());
        result.put("lifetimePoints", updatedAccount.getLifetimePoints());
        result.put("tier", updatedAccount.getTier());
        result.put("tierUpgraded", tierUpgraded);
        
        return result;
    }
    
    @Transactional
    public Map<String, Object> redeemPoints(Long userId, int pointsToRedeem) {
        LoyaltyAccount account = loyaltyAccountRepository.findByUserId(userId)
                .orElseThrow(() -> new RuntimeException("Loyalty account not found"));
        
        if (account.getPoints() < pointsToRedeem) {
            throw new RuntimeException("Insufficient points. Available: " + account.getPoints());
        }
        
        account.setPoints(account.getPoints() - pointsToRedeem);
        LoyaltyAccount updatedAccount = loyaltyAccountRepository.save(account);
        
        // Record transaction
        LoyaltyTransaction transaction = new LoyaltyTransaction();
        transaction.setLoyaltyAccount(updatedAccount);
        transaction.setPoints(-pointsToRedeem);
        transaction.setType(LoyaltyTransaction.TransactionType.REDEEMED);
        transaction.setDescription("Redeemed " + pointsToRedeem + " points");
        
        loyaltyTransactionRepository.save(transaction);
        
        // Calculate discount value (e.g., 100 points = $1)
        @SuppressWarnings("deprecation")
        BigDecimal discountValue = BigDecimal.valueOf(pointsToRedeem).divide(BigDecimal.valueOf(100), 2, BigDecimal.ROUND_HALF_UP);
        
        Map<String, Object> result = new HashMap<>();
        result.put("pointsRedeemed", pointsToRedeem);
        result.put("discountValue", discountValue);
        result.put("remainingPoints", updatedAccount.getPoints());
        
        return result;
    }
    
    @Transactional(readOnly = true)
    public List<Map<String, Object>> getTransactionHistory(Long userId) {
        LoyaltyAccount account = loyaltyAccountRepository.findByUserId(userId)
                .orElseThrow(() -> new RuntimeException("Loyalty account not found"));
        
        List<LoyaltyTransaction> transactions = 
                loyaltyTransactionRepository.findByLoyaltyAccountIdOrderByCreatedAtDesc(account.getId());
        
        return transactions.stream()
                .map(this::convertTransactionToMap)
                .collect(Collectors.toList());
    }
    
    // Discount Management
    @Transactional
    public Map<String, Object> createDiscount(Discount discount) {
        Discount savedDiscount = discountRepository.save(discount);
        return convertDiscountToMap(savedDiscount);
    }
    
    @Transactional(readOnly = true)
    public List<Map<String, Object>> getActiveDiscounts() {
        List<Discount> discounts = discountRepository.findActiveDiscounts(LocalDate.now());
        return discounts.stream()
                .map(this::convertDiscountToMap)
                .collect(Collectors.toList());
    }
    
    @Transactional(readOnly = true)
    public Map<String, Object> validateDiscountCode(String code, Long userId) {
        Discount discount = discountRepository.findByCode(code)
                .orElseThrow(() -> new RuntimeException("Invalid discount code"));
        
        LocalDate now = LocalDate.now();
        
        Map<String, Object> result = new HashMap<>();
        result.put("valid", false);
        result.put("code", code);
        
        if (!discount.getIsActive()) {
            result.put("reason", "Discount is not active");
            return result;
        }
        
        if (now.isBefore(discount.getValidFrom()) || now.isAfter(discount.getValidUntil())) {
            result.put("reason", "Discount has expired or not yet valid");
            return result;
        }
        
        if (discount.getTotalUsageLimit() != null && 
            discount.getCurrentUsageCount() >= discount.getTotalUsageLimit()) {
            result.put("reason", "Discount usage limit reached");
            return result;
        }
        
        if (discount.getMinimumTier() != null || discount.getMinPointsRequired() != null) {
            LoyaltyAccount account = loyaltyAccountRepository.findByUserId(userId)
                    .orElse(null);
            
            if (account == null) {
                result.put("reason", "Loyalty account required for this discount");
                return result;
            }
            
            if (discount.getMinimumTier() != null && 
                account.getTier().ordinal() < discount.getMinimumTier().ordinal()) {
                result.put("reason", "Minimum tier " + discount.getMinimumTier() + " required");
                return result;
            }
            
            if (discount.getMinPointsRequired() != null && 
                account.getPoints() < discount.getMinPointsRequired()) {
                result.put("reason", "Minimum " + discount.getMinPointsRequired() + " points required");
                return result;
            }
        }
        
        result.put("valid", true);
        result.put("discount", convertDiscountToMap(discount));
        
        return result;
    }
    
    @Transactional
    public BigDecimal applyDiscount(String code, BigDecimal originalAmount) {
        Discount discount = discountRepository.findByCode(code)
                .orElseThrow(() -> new RuntimeException("Invalid discount code"));
        
        BigDecimal discountAmount;
        
        if (discount.getType() == Discount.DiscountType.PERCENTAGE) {
            discountAmount = originalAmount.multiply(discount.getValue()).divide(BigDecimal.valueOf(100), 2, BigDecimal.ROUND_HALF_UP);
        } else {
            discountAmount = discount.getValue();
        }
        
        BigDecimal finalAmount = originalAmount.subtract(discountAmount);
        if (finalAmount.compareTo(BigDecimal.ZERO) < 0) {
            finalAmount = BigDecimal.ZERO;
        }
        
        // Update usage count
        discount.setCurrentUsageCount(discount.getCurrentUsageCount() + 1);
        discountRepository.save(discount);
        
        return finalAmount;
    }
    
    private Map<String, Object> convertAccountToMap(LoyaltyAccount account) {
        Map<String, Object> map = new HashMap<>();
        map.put("id", account.getId());
        map.put("userId", account.getUser().getId());
        map.put("userName", account.getUser().getFirstName() + " " + account.getUser().getLastName());
        map.put("points", account.getPoints());
        map.put("lifetimePoints", account.getLifetimePoints());
        map.put("tier", account.getTier());
        map.put("tierBenefits", Map.of(
                "multiplier", account.getTier().getPointsMultiplier(),
                "nextTier", getNextTier(account.getTier()),
                "pointsToNextTier", getPointsToNextTier(account.getLifetimePoints(), account.getTier())
        ));
        map.put("createdAt", account.getCreatedAt());
        return map;
    }
    
    private Map<String, Object> convertTransactionToMap(LoyaltyTransaction transaction) {
        Map<String, Object> map = new HashMap<>();
        map.put("id", transaction.getId());
        map.put("points", transaction.getPoints());
        map.put("type", transaction.getType());
        map.put("description", transaction.getDescription());
        map.put("createdAt", transaction.getCreatedAt());
        
        if (transaction.getReservation() != null) {
            map.put("reservationId", transaction.getReservation().getId());
        }
        
        return map;
    }
    
    private Map<String, Object> convertDiscountToMap(Discount discount) {
        Map<String, Object> map = new HashMap<>();
        map.put("id", discount.getId());
        map.put("code", discount.getCode());
        map.put("description", discount.getDescription());
        map.put("type", discount.getType());
        map.put("value", discount.getValue());
        map.put("minimumTier", discount.getMinimumTier());
        map.put("minPointsRequired", discount.getMinPointsRequired());
        map.put("validFrom", discount.getValidFrom());
        map.put("validUntil", discount.getValidUntil());
        map.put("isActive", discount.getIsActive());
        return map;
    }
    
    private String getNextTier(LoyaltyAccount.MemberTier currentTier) {
        LoyaltyAccount.MemberTier[] tiers = LoyaltyAccount.MemberTier.values();
        int currentIndex = currentTier.ordinal();
        if (currentIndex < tiers.length - 1) {
            return tiers[currentIndex + 1].name();
        }
        return "MAX";
    }
    
    private int getPointsToNextTier(int currentLifetimePoints, LoyaltyAccount.MemberTier currentTier) {
        LoyaltyAccount.MemberTier[] tiers = LoyaltyAccount.MemberTier.values();
        int currentIndex = currentTier.ordinal();
        if (currentIndex < tiers.length - 1) {
            LoyaltyAccount.MemberTier nextTier = tiers[currentIndex + 1];
            return Math.max(0, nextTier.getRequiredPoints() - currentLifetimePoints);
        }
        return 0;
    }
}
