import { useCallback, useEffect, useRef, useState } from 'react';
import type { KeyboardEvent } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { MessageSquare, Send, ArrowLeft, Circle } from 'lucide-react';
import toast from 'react-hot-toast';
import { Button } from '../../components/ui';
import { messagesApi } from '../../api/messages.api';
import { conversationsApi } from '../../api/conversations.api';
import type { Message } from '../../api/messages.api';
import type { Conversation } from '../../api/conversations.api';
import { useSignalR } from '../../hooks/useSignalR';
import type { ReceivedMessage } from '../../hooks/useSignalR';
import { conversationKeys, messageKeys } from '../../lib/queryKeys';
import { useAuthStore } from '../../stores/auth.store';
import { formatRelativeTime } from '../../utils';

import { useCompanyWorkspace } from '../../hooks/company/useCompanyWorkspace';
import {
    CompanyWorkspaceErrorState,
    CompanyWorkspaceLoader,
} from '../../components/features/companies';

export function CompanyMessagesPage() {
    const { conversationId } = useParams<{ conversationId: string }>();
    const navigate = useNavigate();
    const queryClient = useQueryClient();
    const { t } = useTranslation('company');
    const { user } = useAuthStore();
    const [draft, setDraft] = useState('');
    const [typingUser, setTypingUser] = useState<string | null>(null);
    const typingTimeoutRef = useRef<ReturnType<typeof setTimeout>>(undefined);
    const bottomAnchorRef = useRef<HTMLDivElement>(null);
    const lastTypingSentRef = useRef(0);

    // ── SignalR ────────────────────────────────────────────────────────────────
    const signalR = useSignalR({
        onReceiveMessage: (msg: ReceivedMessage) => {
            // Append real-time message into the cache
            queryClient.setQueryData<Message[]>(
                messageKeys.byConversation(msg.conversationId),
                (current = []) => {
                    // Deduplicate: skip if we already have this message (or it replaces an optimistic one)
                    const filtered = current.filter(
                        (m) => !m.id.startsWith('optimistic-') && m.id !== msg.id
                    );
                    return [...filtered, msg as Message];
                }
            );
            // Refresh conversation list for last message preview
            queryClient.invalidateQueries({ queryKey: conversationKeys.list() });
        },
        onConversationUpdated: () => {
            queryClient.invalidateQueries({ queryKey: conversationKeys.list() });
        },
        onMessageRead: (event) => {
            // Update messages in cache to reflect read status
            queryClient.setQueryData<Message[]>(
                messageKeys.byConversation(event.conversationId),
                (current = []) =>
                    current.map((m) =>
                        m.senderId === user?.id && !m.isRead
                            ? { ...m, isRead: true, readAt: new Date().toISOString() }
                            : m
                    )
            );
        },
        onUserTyping: (event) => {
            if (event.conversationId === conversationId) {
                setTypingUser(event.userId);
                clearTimeout(typingTimeoutRef.current);
                typingTimeoutRef.current = setTimeout(() => setTypingUser(null), 3000);
            }
        },
    });

    // ── Join / leave conversation group ────────────────────────────────────────
    const { joinConversation, leaveConversation, markAsRead, isConnected } = signalR;

    useEffect(() => {
        if (!conversationId || !isConnected) return;
        joinConversation(conversationId);
        markAsRead(conversationId);
        
        // Optimistically clear the unread count in conversations list
        queryClient.setQueryData<Conversation[]>(
            conversationKeys.list(),
            (current = []) => current.map(c => 
                c.id === conversationId ? { ...c, unreadCount: 0 } : c
            )
        );
        // Invalidate global unread count to immediately hide sidebar dot
        queryClient.invalidateQueries({ queryKey: conversationKeys.unreadCount() });

        return () => {
            leaveConversation(conversationId);
        };
    }, [conversationId, isConnected, queryClient, joinConversation, markAsRead, leaveConversation]);

    // ── Conversations list ─────────────────────────────────────────────────────
    const conversationsQuery = useQuery({
        queryKey: conversationKeys.list(),
        queryFn: () => conversationsApi.getConversations(),
    });

    // ── Messages for the selected conversation ────────────────────────────────
    const messagesQuery = useQuery({
        queryKey: messageKeys.byConversation(conversationId ?? ''),
        queryFn: () => messagesApi.getMessages(conversationId!),
        enabled: !!conversationId,
        placeholderData: (prev) => prev,
    });

    // ── Auto-scroll ───────────────────────────────────────────────────────────
    useEffect(() => {
        bottomAnchorRef.current?.scrollIntoView({ behavior: 'smooth' });
    }, [messagesQuery.data]);

    // ── Send message ──────────────────────────────────────────────────────────
    const handleSend = useCallback(async () => {
        const text = draft.trim();
        if (!text || !conversationId) return;

        // Optimistic update
        const optimisticMsg: Message = {
            id: `optimistic-${Date.now()}`,
            conversationId,
            senderId: user?.id ?? '',
            text,
            isRead: false,
            createdAt: new Date().toISOString(),
        };

        queryClient.setQueryData<Message[]>(
            messageKeys.byConversation(conversationId),
            (current = []) => [...current, optimisticMsg]
        );
        setDraft('');

        try {
            await signalR.sendMessage(conversationId, text);
        } catch {
            // Rollback optimistic message
            queryClient.setQueryData<Message[]>(
                messageKeys.byConversation(conversationId),
                (current = []) => current.filter((m) => m.id !== optimisticMsg.id)
            );
            toast.error(t('messages.error.sendFailed'));
        }
    }, [draft, conversationId, user?.id, queryClient, signalR, t]);

    // ── Typing indicator ──────────────────────────────────────────────────────
    const handleTyping = useCallback(() => {
        if (!conversationId) return;
        const now = Date.now();
        // Throttle: send at most once every 2 seconds
        if (now - lastTypingSentRef.current > 2000) {
            lastTypingSentRef.current = now;
            signalR.sendTyping(conversationId);
        }
    }, [conversationId, signalR]);

    const handleKeyDown = (event: KeyboardEvent<HTMLTextAreaElement>) => {
        if (event.key === 'Enter' && !event.shiftKey) {
            event.preventDefault();
            handleSend();
        }
    };

    const {
        isLoading: isWorkspaceLoading,
        hasFatalError,
    } = useCompanyWorkspace();

    if (isWorkspaceLoading) return <CompanyWorkspaceLoader />;

    if (hasFatalError) {
        return (
            <CompanyWorkspaceErrorState
                title={t('messages.errorTitle')}
                description={t('messages.errorDesc')}
                icon={<MessageSquare className="h-6 w-6" />}
            />
        );
    }

    // ── Render ─────────────────────────────────────────────────────────────────
    const conversations = conversationsQuery.data ?? [];
    const messages = messagesQuery.data ?? [];

    return (
        <div className="flex h-[calc(100vh-10rem)] gap-0 overflow-hidden rounded-2xl border border-gray-200 bg-white shadow-sm">
            {/* ── Sidebar: conversation list ──────────────────────────────── */}
            <div
                className={[
                    'flex w-full flex-col border-r border-gray-100 md:w-[340px] md:min-w-[340px]',
                    conversationId ? 'hidden md:flex' : 'flex',
                ].join(' ')}
            >
                <div className="border-b border-gray-100 px-5 py-4">
                    <h2 className="text-lg font-semibold text-gray-900">{t('messages.title')}</h2>
                    <div className="mt-1 flex items-center gap-2 text-xs text-gray-400">
                        <Circle
                            className={`h-2 w-2 fill-current ${signalR.isConnected ? 'text-emerald-500' : 'text-gray-300'}`}
                        />
                        {signalR.isConnected ? t('messages.status.connected') : t('messages.status.connecting')}
                    </div>
                </div>

                <div className="flex-1 overflow-y-auto">
                    {conversationsQuery.isLoading ? (
                        <div className="flex items-center justify-center py-12">
                            <div className="h-6 w-6 animate-spin rounded-full border-2 border-[#00b14f] border-t-transparent" />
                        </div>
                    ) : conversations.length === 0 ? (
                        <div className="px-5 py-12 text-center text-sm text-gray-400">
                            {t('messages.conversations.empty')}
                        </div>
                    ) : (
                        conversations.map((conv) => (
                            <ConversationItem
                                key={conv.id}
                                conversation={conv}
                                isActive={conv.id === conversationId}
                                onClick={() => navigate(`/company/messages/${conv.id}`)}
                            />
                        ))
                    )}
                </div>
            </div>

            {/* ── Chat panel ──────────────────────────────────────────────── */}
            <div
                className={[
                    'flex flex-1 flex-col',
                    conversationId ? 'flex' : 'hidden md:flex',
                ].join(' ')}
            >
                {!conversationId ? (
                    <div className="flex flex-1 items-center justify-center">
                        <div className="text-center">
                            <MessageSquare className="mx-auto h-12 w-12 text-gray-200" />
                            <p className="mt-3 text-sm text-gray-400">
                                {t('messages.chat.selectPrompt')}
                            </p>
                        </div>
                    </div>
                ) : (
                    <>
                        {/* Chat header */}
                        <ChatHeader
                            conversation={conversations.find((c) => c.id === conversationId)}
                            onBack={() => navigate('/company/messages')}
                        />

                        {/* Message list */}
                        <div className="flex-1 overflow-y-auto px-5 py-4">
                            {messagesQuery.isLoading && !messagesQuery.data ? (
                                <div className="flex h-full items-center justify-center">
                                    <div className="h-8 w-8 animate-spin rounded-full border-b-2 border-[#00b14f]" />
                                </div>
                            ) : messages.length === 0 ? (
                                <div className="flex h-full items-center justify-center">
                                    <p className="text-sm text-gray-400">
                                        {t('messages.chat.empty')}
                                    </p>
                                </div>
                            ) : (
                                <div className="space-y-2">
                                    {messages.map((msg) => (
                                        <MessageBubble
                                            key={msg.id}
                                            message={msg}
                                            isSelf={msg.senderId === user?.id}
                                        />
                                    ))}
                                </div>
                            )}
                            {typingUser && (
                                <div className="mt-2 text-xs text-gray-400 italic">
                                    {t('messages.chat.typing')}
                                </div>
                            )}
                            <div ref={bottomAnchorRef} />
                        </div>

                        {/* Input */}
                        <div className="border-t border-gray-100 px-5 py-3">
                            <div className="flex items-end gap-3">
                                <textarea
                                    rows={2}
                                    value={draft}
                                    onChange={(e) => {
                                        setDraft(e.target.value);
                                        handleTyping();
                                    }}
                                    onKeyDown={handleKeyDown}
                                    placeholder={t('messages.input.placeholder')}
                                    className="flex-1 resize-none rounded-xl border border-gray-200 px-4 py-2.5 text-sm text-gray-900 outline-none transition placeholder:text-gray-400 focus:border-[#00b14f] focus:ring-2 focus:ring-[#00b14f]/20"
                                />
                                <Button
                                    onClick={handleSend}
                                    disabled={!draft.trim()}
                                    aria-label={t('messages.input.send')}
                                >
                                    <Send className="h-4 w-4" />
                                    {t('messages.input.send')}
                                </Button>
                            </div>
                            <p className="mt-1.5 text-[11px] text-gray-400">
                                {t('messages.input.hint')}
                            </p>
                        </div>
                    </>
                )}
            </div>
        </div>
    );
}

// ── Sub-components ────────────────────────────────────────────────────────────

function ConversationItem({
    conversation,
    isActive,
    onClick,
}: {
    conversation: Conversation;
    isActive: boolean;
    onClick: () => void;
}) {
    return (
        <button
            type="button"
            onClick={onClick}
            className={[
                'flex w-full items-center gap-3 px-5 py-3.5 text-left transition-colors',
                isActive
                    ? 'bg-[#00b14f]/5 border-r-2 border-[#00b14f]'
                    : 'hover:bg-gray-50',
            ].join(' ')}
        >
            <div className="relative h-10 w-10 flex-shrink-0">
                {conversation.otherUserAvatar ? (
                    <img
                        src={conversation.otherUserAvatar}
                        alt=""
                        className="h-10 w-10 rounded-full object-cover"
                    />
                ) : (
                    <div className="flex h-10 w-10 items-center justify-center rounded-full bg-gradient-to-br from-[#00b14f] to-emerald-600 text-sm font-semibold text-white">
                        {conversation.otherUserName?.charAt(0)?.toUpperCase() ?? '?'}
                    </div>
                )}
                {conversation.unreadCount > 0 && (
                    <span className="absolute -right-0.5 -top-0.5 flex h-5 min-w-5 items-center justify-center rounded-full bg-red-500 px-1 text-[10px] font-bold text-white">
                        {conversation.unreadCount > 99 ? '99+' : conversation.unreadCount}
                    </span>
                )}
            </div>

            <div className="min-w-0 flex-1">
                <div className="flex items-baseline justify-between gap-2">
                    <p
                        className={`truncate text-sm ${
                            conversation.unreadCount > 0 ? 'font-semibold text-gray-900' : 'font-medium text-gray-700'
                        }`}
                    >
                        {conversation.otherUserName}
                    </p>
                    {conversation.lastMessageAt && (
                        <span className="flex-shrink-0 text-[11px] text-gray-400">
                            {formatRelativeTime(conversation.lastMessageAt)}
                        </span>
                    )}
                </div>
                {conversation.lastMessagePreview && (
                    <p
                        className={`mt-0.5 truncate text-xs ${
                            conversation.unreadCount > 0 ? 'font-medium text-gray-700' : 'text-gray-400'
                        }`}
                    >
                        {conversation.lastMessagePreview}
                    </p>
                )}
            </div>
        </button>
    );
}

function ChatHeader({
    conversation,
    onBack,
}: {
    conversation?: Conversation;
    onBack: () => void;
}) {
    const { t } = useTranslation('company');
    return (
        <div className="flex items-center gap-3 border-b border-gray-100 px-5 py-3">
            <button
                type="button"
                onClick={onBack}
                className="rounded-lg p-1.5 text-gray-400 transition hover:bg-gray-100 hover:text-gray-600 md:hidden"
                aria-label={t('messages.header.back')}
            >
                <ArrowLeft className="h-5 w-5" />
            </button>
            {conversation && (
                <>
                    {conversation.otherUserAvatar ? (
                        <img
                            src={conversation.otherUserAvatar}
                            alt=""
                            className="h-9 w-9 rounded-full object-cover"
                        />
                    ) : (
                        <div className="flex h-9 w-9 items-center justify-center rounded-full bg-gradient-to-br from-[#00b14f] to-emerald-600 text-sm font-semibold text-white">
                            {conversation.otherUserName?.charAt(0)?.toUpperCase() ?? '?'}
                        </div>
                    )}
                    <p className="text-sm font-semibold text-gray-900">
                        {conversation.otherUserName}
                    </p>
                </>
            )}
        </div>
    );
}

function MessageBubble({ message, isSelf }: { message: Message; isSelf: boolean }) {
    const { t } = useTranslation('company');
    const isOptimistic = message.id.startsWith('optimistic-');
    return (
        <div className={`flex ${isSelf ? 'justify-end' : 'justify-start'}`}>
            <div
                className={[
                    'max-w-[70%] rounded-2xl px-4 py-2.5 transition-opacity',
                    isSelf ? 'bg-[#00b14f] text-white' : 'bg-gray-100 text-gray-900',
                    isOptimistic ? 'opacity-60' : 'opacity-100',
                ].join(' ')}
            >
                <p className="whitespace-pre-wrap text-sm leading-6">{message.text}</p>
                <p
                    className={`mt-1 text-right text-[10px] ${
                        isSelf ? 'text-white/70' : 'text-gray-400'
                    }`}
                >
                    {isOptimistic ? t('messages.bubble.sending') : formatRelativeTime(message.createdAt)}
                    {isSelf && !isOptimistic && message.isRead && ` · ${t('messages.bubble.read')}`}
                </p>
            </div>
        </div>
    );
}

