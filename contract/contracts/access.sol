//SPDX-License-Identifier: Unlicense
pragma solidity ^0.8.0;

contract Access {
    address private owner;
    uint256 private adminVersion = 0;
    mapping(uint256 => bool) private adminExists;

    constructor() {
        owner = msg.sender;
        address[] memory users = new address[](1);
        users[0] = owner;
        _addAdmins(users);
    }

    // 权限相关
    modifier onlyOwner() {
        require(msg.sender == owner, "O");
        _;
    }
    modifier onlyAdmin() {
        require(adminExists[userKey(msg.sender)], "A");
        _;
    }

    function addAdmins(address[] calldata users) public onlyOwner {
        _addAdmins(users);
    }

    function addAdmin(address user) public onlyOwner {
        _addAdmin(user);
    }

    function _addAdmin(address user) internal {
        adminExists[userKey(user)] = true;
    }

    function _addAdmins(address[] memory users) internal {
        for (uint i = 0; i < users.length; i++) {
            adminExists[userKey(users[i])] = true;
        }
    }

    function removeAdmins(address[] calldata users) public onlyOwner {
        for (uint i = 0; i < users.length; i++) {
            adminExists[userKey(users[i])] = false;
        }
    }

    function clearAdmin() public onlyOwner {
        adminVersion++;
        address[] memory users = new address[](1);
        users[0] = owner;
        _addAdmins(users);
    }

    function userKey(address user) internal view returns (uint256) {
        return uint256(uint160(user)) * 10000 + adminVersion;
    }
}
