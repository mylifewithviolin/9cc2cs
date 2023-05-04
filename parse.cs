using System; 
using System.Text.RegularExpressions;



namespace CC9
{
    public partial class Program
    {

        const string TOKEN_SYNBL_PLUS ="+";
        const string TOKEN_SYNBL_MINUS ="-";

        // ��ݹ�ʸ�ڤΥΡ��ɤμ���
        enum NodeKind{
            ND_ADD, // +
            ND_SUB, // -
            ND_MUL, // *
            ND_DIV, // /
            /// <summary>��������</summary>
            ND_ASSIGN, // =
            /// <summary>�������ѿ�</summary>
            ND_LVAR,   // 1ʸ���ѿ�
            ND_EQ,  // ==
            ND_NE,  // !=
            ND_LT,  // <
            ND_LE,  // <=
            ND_EXPR_STMT, // ; Expression statement
            ND_NUM, // ����
        };

    
        // ��ݹ�ʸ�ڤΥΡ��ɤη�
        class  Node {
            public NodeKind kind; // �Ρ��ɤη�
            public Node? lhs;     // ����
            public Node? rhs;     // ����
            public int val;       // kind��ND_NUM�ξ��Τ߻Ȥ�
            /// <summary>���ե��å���</summary>
            public int offset;    // kind��ND_LVAR�ξ��Τ߻Ȥ�
        };

        
        /// <summary>��³����</summary>
        /// <param name="codeList">�����ɥꥹ��</param>
        /// <param name="tokenList">�ȡ�����ꥹ��</param>
        /// <param name="curIndex">������</param>
        static void program(List<Node> codeList,List<Token> tokenList,ref int curIndex){
            Token token = getToken(tokenList,curIndex);//���Υȡ�����
            while (!at_eof(token)){
                Node code = stmt(tokenList,ref curIndex);
                codeList.Add(code);
                token = getToken(tokenList,curIndex);//���Υȡ�����
            }
        }
        /// <summary>��ʸ</summary>
        /// <param name="tokenList">�ȡ�����ꥹ��</param>
        /// <param name="curIndex">������</param>
        /// <returns>�Ρ��ɷ�</returns>
        static Node stmt(List<Token> tokenList,ref int curIndex){
            Node node = expr(tokenList,ref curIndex);
            Token token = getToken(tokenList,curIndex);//���Υȡ�����
            expect(token,";",ref curIndex);
            return node;
        }
        /// <summary>��</summary>
        /// <param name="tokenList">�ȡ�����ꥹ��</param>
        /// <param name="curIndex">������</param>
        /// <returns>����</returns>
        static Node expr(List<Token> tokenList,ref int curIndex) {
            return assign(tokenList,ref curIndex);
        }

        /// <summary>������</summary>
        /// <param name="tokenList">�ȡ�����ꥹ��</param>
        /// <param name="curIndex">������</param>
        /// <returns>�Ρ��ɷ�</returns>
        static Node assign(List<Token> tokenList,ref int curIndex) {
            Node node = equality(tokenList,ref curIndex);
            Token token = getToken(tokenList,curIndex);//���Υȡ�����
            if (consume(token,"=",ref curIndex)){
                node = new_binary(NodeKind.ND_ASSIGN, node, assign(tokenList,ref curIndex));
            }
            return node;
        }

        /// <summary>����</summary>
        /// <param name="tokenList">�ȡ�����ꥹ��</param>
        /// <param name="curIndex">������</param>
        /// <returns>�Ρ���</returns>
        static Node equality(List<Token> tokenList,ref int curIndex) {
            Node node = relational(tokenList,ref curIndex);
            Token token = getToken(tokenList,curIndex);//���Υȡ�����
            for (;;) {
                if (consume(token,"-=",ref curIndex)){
                    node = new_binary(NodeKind.ND_EQ, node, relational(tokenList,ref curIndex));
                    token = getToken(tokenList,curIndex);//���Υȡ�����
                }
                else if (consume(token,"!=",ref curIndex)){
                    node = new_binary(NodeKind.ND_NE, node, relational(tokenList,ref curIndex));
                    token = getToken(tokenList,curIndex);//���Υȡ�����
                }
                else
                return node;
            }
        }

        /// <summary>�ط���</summary>
        /// <param name="tokenList">�ȡ�����ꥹ��</param>
        /// <param name="curIndex">������</param>
        /// <returns>�Ρ���</returns>
        static Node relational(List<Token> tokenList,ref int curIndex) {
            Node node = add(tokenList,ref curIndex);
            Token token = getToken(tokenList,curIndex);//���Υȡ�����
            for (;;) {
                if (consume(token,"<",ref curIndex)){
                                    node = new_binary(NodeKind.ND_LT, node, primary(tokenList,ref curIndex));
                                token = getToken(tokenList,curIndex);//���Υȡ�����
                }
                else if (consume(token,"<=",ref curIndex)){
                    node = new_binary(NodeKind.ND_LE, node, primary(tokenList,ref curIndex));
                    token = getToken(tokenList,curIndex);//���Υȡ�����
                    }
                else if (consume(token,">",ref curIndex)){
                    node = new_binary(NodeKind.ND_LT, add(tokenList,ref curIndex), node);
                    token = getToken(tokenList,curIndex);//���Υȡ�����
                    }
                else if (consume(token,">=",ref curIndex)){
                    node = new_binary(NodeKind.ND_LE, add(tokenList,ref curIndex), node);
                    token = getToken(tokenList,curIndex);//���Υȡ�����
                    }
                else
                return node;
            }
        }
        /// <summary>�ø���</summary>
        /// <param name="tokenList">�ȡ�����ꥹ��</param>
        /// <param name="curIndex">������</param>
        /// <returns>�Ρ���</returns>
        static Node add(List<Token> tokenList,ref int curIndex) {
            Node node = mul(tokenList,ref curIndex);
            Token token = getToken(tokenList,curIndex);//���Υȡ�����
            for (;;) {
                if (consume(token,"+",ref curIndex)){
                    //node = new_node(NodeKind.ND_ADD, node, mul(tokenList,ref curIndex));
                    node = new_binary(NodeKind.ND_ADD, node, mul(tokenList,ref curIndex));
                    token = getToken(tokenList,curIndex);//���Υȡ�����
                    }
                else if (consume(token,"-",ref curIndex)){
                    //node = new_node(NodeKind.ND_SUB, node, mul(tokenList,ref curIndex));
                    node = new_binary(NodeKind.ND_SUB, node, mul(tokenList,ref curIndex));
                    token = getToken(tokenList,curIndex);//���Υȡ�����
                    }
                else
                    return node;
            }
        }
        /// <summary>�����</summary>
        /// <param name="tokenList">�ȡ�����ꥹ��</param>
        /// <param name="curIndex">������</param>
        /// <returns>�Ρ���</returns>
        static Node mul(List<Token> tokenList,ref int curIndex) {
            //Node node = primary(tokenList,ref curIndex);
            Node node = unary(tokenList,ref curIndex);
            Token token = getToken(tokenList,curIndex);//���Υȡ�����
            for (;;) {
                if (consume(token,"*",ref curIndex)){
                    //node = new_node(NodeKind.ND_MUL, node, primary(tokenList,ref curIndex));
                    node = new_binary(NodeKind.ND_MUL, node, primary(tokenList,ref curIndex));
                    token = getToken(tokenList,curIndex);//���Υȡ�����
                }
                else if (consume(token,"/",ref curIndex)){
                    //node = new_node(NodeKind.ND_DIV, node, primary(tokenList,ref curIndex));
                    node = new_binary(NodeKind.ND_DIV, node, primary(tokenList,ref curIndex));
                    token = getToken(tokenList,curIndex);//���Υȡ�����
                }
                else
                return node;
            }
        }
        /// <summary>ñ�༰</summary>
        /// <param name="tokenList">�ȡ�����ꥹ��</param>
        /// <param name="curIndex">������</param>
        /// <returns>�Ρ���</returns>
        static Node unary(List<Token> tokenList,ref int curIndex) { 
                Token token = getToken(tokenList,curIndex);//���Υȡ�����
                if (consume(token,"+",ref curIndex)){
                    return unary(tokenList,ref curIndex);
                }
                else if (consume(token,"-",ref curIndex)){
                    return new_binary(NodeKind.ND_SUB,new_num(0), unary(tokenList,ref curIndex));
                }
                else
                return primary(tokenList,ref curIndex);
        }
        /// <summary>ͥ����</summary>
        /// <param name="tokenList">�ȡ�����ꥹ��</param>
        /// <param name="curIndex">������</param>
        /// <returns>�Ρ���</returns>
        static Node primary(List<Token> tokenList,ref int curIndex) {
            Token token = getToken(tokenList,curIndex);//���Υȡ�����
            // ���Υȡ�����"("�ʤ顢"(" expr ")"�ΤϤ�
            if (consume(token,"(",ref curIndex)) {
                Node node = expr(tokenList,ref curIndex);
                token = getToken(tokenList,curIndex);//���Υȡ�����
                expect(token,")",ref curIndex);
                token = getToken(tokenList,curIndex);//���Υȡ�����
                return node;
            }

            // �����Ǥʤ���м��̻�
            if (token.kind == TokenKind.TK_IDENT) {
                Node node = new_node(NodeKind.ND_LVAR);
                node.offset = expect_identifer(token,ref curIndex);
                return node;
            }

            // �����Ǥʤ���п��ͤΤϤ�
            //return new_node_num(expect_number(token,ref curIndex));
            return new_num(expect_number(token,ref curIndex));
        }

            static Token getToken(List<Token> tokenList,int curIndex) {
                if(curIndex>=tokenList.Count) return new Token();
                Token token = tokenList.ElementAt(curIndex);//���Υȡ�����
                return token;
            }
            /// <summary>�������Ρ���</summary>
            /// <param name="kind">����</param>
            /// <returns>�Ρ���</returns>
            static Node new_node(NodeKind kind) {
                Node node = new();
                node.kind = kind;
                // node.lhs = lhs;
                // node.rhs = rhs;
                return node;
            }
            /// <summary>��������ʬ��</summary>
            /// <param name="kind">����</param>
            /// <param name="lhs">����</param>
            /// <param name="rhs">����</param>
            /// <returns>node</returns>
            static Node new_binary(NodeKind kind, Node lhs, Node rhs) {
                Node node = new_node(kind);
                node.lhs = lhs;
                node.rhs = rhs;
                return node;
            }


            static Node new_num(int val) {
                Node node = new_node(NodeKind.ND_NUM);
                node.val = val;
                return node;
            }


            // ���Υȡ����󤬴��Ԥ��Ƥ��뵭��ΤȤ��ˤϡ��ȡ������1���ɤ߿ʤ��
            // �����֤�������ʳ��ξ��ˤϵ����֤���
            /// <summary>���Ѥ���</summary>
            /// <param name="token">�ȡ�����</param>
            /// <param name="op">����ʸ��</param>
            /// <param name="next">������</param>
            /// <returns>bool</returns>
            static bool consume(Token token,string op, ref int next) {
                //if (token.kind != TokenKind.TK_RESERVED || token.str != op)
                if (token.kind != TokenKind.TK_RESERVED || op.Length  != token.len || 
                        token.str != op)
                    return false;
                next = token.next;
                return true;
            }
            // ���Υȡ����󤬴��Ԥ��Ƥ��뵭��ΤȤ��ˤϡ��ȡ������1���ɤ߿ʤ�롣
            // ����ʳ��ξ��ˤϥ��顼����𤹤롣
            /// <summary>ͽ�ۤ���</summary>
            /// <param name="token">�ȡ�����</param>
            /// <param name="op">����ʸ��</param>
            /// <param name="next">������</param>
            /// <returns>bool</returns>
        static  void expect(Token token,string op, ref int next) {
            if (token.kind != TokenKind.TK_RESERVED || op.Length  != token.len ||  token.str != op){
                       Console.WriteLine($"{op}�ǤϤ���ޤ���\n");
            }
            next = token.next;
        }

        // ���Υȡ����󤬿��ͤξ�硢�ȡ������1���ɤ߿ʤ�Ƥ��ο��ͤ��֤���
        // ����ʳ��ξ��ˤϥ��顼����𤹤롣
        static int expect_number(Token token, ref int next) {
            if (token.kind != TokenKind.TK_NUM){
                Console.WriteLine($"���ǤϤ���ޤ���\n");
            }
            int val = token.val;
            next = token.next;
            return val;
        }

        // ���Υȡ����󤬼��̻Ҥξ�硢�ȡ������1���ɤ߿ʤ�Ƥ��Υ��ե��å��ͤ��֤���
        // ����ʳ��ξ��ˤϥ��顼����𤹤롣
        static int expect_identifer(Token token, ref int next) {
            if (token.kind != TokenKind.TK_IDENT){
                Console.WriteLine($"���̻ҤǤϤ���ޤ���\n");
            }
            Char ident =char.Parse(token.str);
            var offset = ( ident - 'a' + 1) * 8;
            next = token.next;
            return offset;
        }
        // ���顼����𤹤뤿��δؿ�
        static void error(string fmt) {
            Console.WriteLine(fmt);
        }
        static bool at_eof(Token token) {
            return token.kind == TokenKind.TK_EOF;
        }

        // �������ȡ�������������cur�˷Ҥ���
            static Token new_token(TokenKind kind, int next, string str, int len) {
            Token tok = new();
            tok.kind = kind;
            tok.str = str;
            tok.len = len;
            tok.next = next;
            return tok;
        }
        static bool isspace( string input )
        {
            return( Regex.IsMatch( input,"\\s" ) );
        }
        static bool isarithmetic( string input )
        {
            return( Regex.IsMatch( input,"[\\+\\-\\*\\/\\(\\)\\<\\>]" ) );
        }
        static bool iscomparisonhead ( string input )
        {
            if(input =="="||input =="!"||input =="<"||input ==">"){
                return true;
            }else{
                return false;
            }       
        }
        static bool iscomparison ( string input )
        {
            if(input =="=="||input =="!="||input =="<="||input ==">="){
                return true;
            }else{
                return false;
            }       
        }
        static bool isassign ( string input )
        {
            if(input =="="){
                return true;
            }else{
                return false;
            }       
        }
        static bool isdigit( string input )
        {
            return Regex.IsMatch( input, "[0-9]" );
        }
        static bool isidentfer( string input )
        {
            return Regex.IsMatch( input, "[a-z]" );
        }
        static bool isterminater( string input )
        {
            if(input ==";"){
                return true;
            }else{
                return false;
            }    
        }
        static int strtol(string str){
            return  int.Parse(str);
        }

            
    }
}